using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace AzureStorageCSVFileExtract
{
    /// <summary>
    /// AzureStorageCSVFileExtract
    /// </summary>
    public class AzureStorageCSVFileExtract : IAzureStorageCSVFileExtract
    {
        public BlobServiceClient BlobServiceClient { get; private set; }
        public BlobContainerClient ContainerClient { get; private set; }

        

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public AzureStorageCSVFileExtract()
        {
            BlobServiceClient = new BlobServiceClient(ConfigValue.ConnetionString);
            ContainerClient = BlobServiceClient.GetBlobContainerClient(ConfigValue.ContainerName);
        }

        /// <summary>
        /// GetBlobClient
        /// </summary>
        /// <param name="targetBlobName">BLOB名</param>
        /// <returns>BlobClient</returns>
        public BlobClient GetBlobClient(string targetBlobName)
        {
            return ContainerClient.GetBlobClient(targetBlobName);
        }

        /// <summary>
        /// GetBlobFileStreamAsync
        /// </summary>
        /// <param name="blobClient">BlobClient</param>
        /// <returns>BLOBのファイルストリーム</returns>
        public async Task<Stream> GetBlobFileStreamAsync(BlobClient blobClient)
        {
            return await blobClient.OpenReadAsync();
        }

        /// <summary>
        /// ストリームから、先頭・末尾に対してそれぞれ抽出件数分を取得する
        /// </summary>
        /// <param name="stream">検索するストリーム</param>
        /// <param name="extractCount">抽出件数</param>
        /// <returns>文字列列挙</returns>
        public async Task<IEnumerable<string>> GetExtractTextAsync(Stream stream, int extractCount)
        {
            var results = new List<string>();
            var queue = new Queue<string>(extractCount);
            int count = 0;

            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                var headline = sr.ReadLine();

                //先頭
                while (count < extractCount)
                {
                    if (sr.EndOfStream)
                    {
                        break;
                    }
                    results.Add(await sr.ReadLineAsync());
                    count++;
                }

                //末尾
                while (!sr.EndOfStream)
                {
                    if (queue.Count == extractCount)
                    {
                        queue.Dequeue();
                    }

                    queue.Enqueue(await sr.ReadLineAsync());
                }
            }

            if (queue.Count > 0)
            {
                results.AddRange(queue.ToList());
                queue.Clear();
            }

            return results;
        }

        public async Task<IEnumerable<string>> GetExtractCSVTextAsync(Stream stream, int extractCount)
        {
            var results = new List<string>();
            var queue = new Queue<string>(extractCount);
            int count = 0;

            using (var sr = new StreamReader(stream, Encoding.UTF8))
            {
                var headline = sr.ReadLine();

                //先頭
                while (count < extractCount)
                {
                    if (sr.EndOfStream)
                    {
                        break;
                    }

                    var line = await sr.ReadLineWithDelimiterAsync(ConfigValue.LF);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        results.Add(line.Replace(ConfigValue.CR, string.Empty));
                        count++;
                    }
                }

                //末尾
                while (!sr.EndOfStream)
                {
                    if (queue.Count == extractCount)
                    {
                        queue.Dequeue();
                    }

                    var line = await sr.ReadLineWithDelimiterAsync(ConfigValue.LF);
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        queue.Enqueue(line.Replace(ConfigValue.CR, string.Empty));
                    }
                }
            }

            if (queue.Count > 0)
            {
                results.AddRange(queue.ToList());
                queue.Clear();
            }

            return results;
        }

        /// <summary>
        /// 文字列列挙をファイル保存する
        /// </summary>
        /// <param name="csvStringLists">ピックアップした、レコード</param>
        /// <param name="saveFileFullName">ファイル保存先FullPath</param>
        /// <returns>void</returns>
        public async Task WriteExtractTextAsync(IEnumerable<string> csvStringLists, string saveFileFullName)
        {
            using (var file = new StreamWriter(saveFileFullName, false, Encoding.UTF8))
            {
                foreach (string line in csvStringLists)
                {
                    await file.WriteLineAsync(line);
                }
                await file.FlushAsync();
            }
        }

        /// <summary>
        /// 非同期処理
        /// 指定テーブルのBlobClientから、ストリームを読み込み、テキストを抽出し、ファイル保存する処理の一列の作業タスク
        /// </summary>
        /// <param name="target">指定テーブル</param>
        /// <returns>指定テーブル, タスク処理結果</returns>
        public async Task<Tuple<string, bool>> ExtractTargetAsnc(string target)
        {
            var blobFullPath = ConfigValue.GetTargetBlobName(target);
            Console.WriteLine($"working ... => {target}");

            var result = false;
            var csvSaveFullName = ConfigValue.GetSaveFileName(target);

            var blobClient = GetBlobClient(blobFullPath);
            if (blobClient != null)
            {
                //全てのタスクを非同期処理する。
                var stream = await GetBlobFileStreamAsync(blobClient);
                IEnumerable<string> extractList;

                if (ConfigValue.IsExtractTargetIncludeCR(target))
                {
                    extractList = await GetExtractCSVTextAsync(stream, ConfigValue.ExtractCount);
                }
                else
                {
                    extractList = await GetExtractTextAsync(stream, ConfigValue.ExtractCount);
                }

                if (extractList.Count() > 0)
                {
                    await WriteExtractTextAsync(extractList, csvSaveFullName);
                }

                result = true;
                Console.WriteLine($"complete... => Table:[{target}], Count:[{extractList.Count()}], TaskWork:[{result}]");
            }

            return Tuple.Create(target, result);
        }

        /// <summary>
        /// 同期処理
        /// 指定テーブルのBlobClientから、ストリームを読み込み、テキストを抽出し、ファイル保存する処理の一列の作業タスク
        /// </summary>
        /// <param name="target">指定テーブル</param>
        /// <returns>指定テーブル, タスク処理結果</returns>
        public Task<Tuple<string, bool>> ExtractTarget(string target)
        {
            Console.WriteLine($"working ... => {target}");

            var result = false;
            var targetBlobName = ConfigValue.GetTargetBlobName(target);
            var blobClient = GetBlobClient(targetBlobName);

            if (blobClient != null)
            {
                //全てのタスクを非同期処理する。
                var stream = GetBlobFileStreamAsync(blobClient).Result;
                var extractList = GetExtractTextAsync(stream, ConfigValue.ExtractCount).Result;

                if (extractList.Count() > 0)
                {
                    WriteExtractTextAsync(extractList, ConfigValue.GetSaveFileName(target)).Wait();
                }

                result = true;
                Console.WriteLine($"complete ... => Table:[{target}], Count:[{extractList.Count()}], TaskWork:[{result}]");
            }

            return Task.FromResult(Tuple.Create(target, result));
        }
    }
}
