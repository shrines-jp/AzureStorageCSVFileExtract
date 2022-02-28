using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace AzureStorageCSVFileExtract
{
    /// <summary>
    /// Azure BlobStorageから先頭・末尾100件抽出するツール
    /// 
    /// 作成日：2022/01/27 
    /// 作成者：AIS 朴
    /// 機能：
    ///     Azure Blob Storageに格納されている、第4世代データベースから抽出したCSVファイルを読み込み。
    ///     先頭・末尾100件をピックアップして、別のファイルへ保存する。
    /// 処理：非同期処理
    /// </summary>
    class Program
    {
        private static readonly string targetLists = ConfigurationManager.AppSettings.Get("ExtractTargets") ?? string.Empty;
        private static readonly string targetREPLists = ConfigurationManager.AppSettings.Get("ExtractTargetsREP") ?? string.Empty;

        static async Task Main(string[] args)
        {
            /*
            // iko/CSV/EAT001.csv
                Blob Container : iko
                BlobPath : CSV
                BlobFileName : EAT001.csv

                BlockBlob : 
                    Name : CSV/EAT001.csv
            */

            Console.WriteLine($"program start:");

            //非同期処理（同期処理）
            //await Task.Run(() => FileExtract());

            //非同期処理
            await FileExtractAsync();

            Console.WriteLine($"exit this program by presing any key:");
            Console.ReadKey();
            Environment.Exit(0);
        }

        /// <summary>
        /// 同期処理
        /// </summary>
        private static void FileExtract()
        {
            try
            {
                var extractWorker = new AzureStorageCSVFileExtract();
                var count = 0;
                foreach (var target in targetLists.Split(','))
                {
                    Console.WriteLine($"Starting Process {target}");
                    var task = extractWorker.ExtractTarget(target).Result;

                    if (task.Item2)
                    {
                        Console.WriteLine($"Ending Process {task.Item1}{Environment.NewLine}");
                    }

                    count++;
                }
                Console.WriteLine($"All Process is Done... {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// 非同期処理
        /// </summary>
        /// <returns></returns>
        private static async Task FileExtractAsync()
        {
            try
            {
                // Tuple<string, bool> テーブル名、タスク処理可否
                var tasks = new List<Task<Tuple<string, bool>>>();
                var extractWorker = new AzureStorageCSVFileExtract();

                foreach (var target in targetLists.Split(','))
                {
                    var repTarget = ConfigValue.IsExtractTargetREP(target) ? string.Concat(target, "_REP") : target;
                    Console.WriteLine($"Starting Process {repTarget}");
                    tasks.Add(extractWorker.ExtractTargetAsnc(repTarget));
                }

                foreach (var task in await Task.WhenAll(tasks))
                {
                    if (task.Item2)
                    {
                        Console.WriteLine($"Ending Process {task.Item1}{Environment.NewLine}");
                    }
                }

                Console.WriteLine($"All Process is Done... {tasks.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
