using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace AzureStorageCSVFileExtract
{
    /// <summary>
    /// IAzureStorageCSVFileExtract
    /// </summary>
    public interface IAzureStorageCSVFileExtract
    {
        /// <summary>
        /// The Azure.Storage.Blobs.BlobServiceClient allows you to manipulate Azure Storage
        /// service resources and blob containers. 
        /// </summary>
        BlobServiceClient BlobServiceClient { get; }

        /// <summary>
        /// The Azure.Storage.Blobs.BlobContainerClient allows you to manipulate Azure Storage
        /// containers and their blobs. 
        /// </summary>
        BlobContainerClient ContainerClient { get; }

        /// <summary>
        /// Create a new Azure.Storage.Blobs.BlobClient object by appending blobName to the 
        /// end of Azure.Storage.Blobs.BlobContainerClient.Uri.
        /// </summary>
        /// <param name="targetBlobName">The name of the blob.</param>
        /// <returns>A new Azure.Storage.Blobs.BlobClient instance.</returns>
        BlobClient GetBlobClient(string targetBlobName);

        /// <summary>
        /// Opens a stream for reading from the blob. The stream will only download the blob as the stream is read from.
        /// </summary>
        /// <param name="blobClient">A new Azure.Storage.Blobs.BlobClient instance.</param>
        /// <returns>Returns a stream that will download the blob as the stream is read from.</returns>
        Task<Stream> GetBlobFileStreamAsync(BlobClient blobClient);

        /// <summary>
        /// ストリームから、先頭・末尾に対してそれぞれ抽出件数分を取得する
        /// </summary>
        /// <param name="stream">検索するストリーム</param>
        /// <param name="extractCount">抽出件数</param>
        /// <returns>文字列列挙</returns>
        Task<IEnumerable<string>> GetExtractTextAsync(Stream stream, int extractCount);

        /// <summary>
        /// 文字列列挙をファイル保存する
        /// </summary>
        /// <param name="csvStringLists">ピックアップした、レコード</param>
        /// <param name="saveFileFullName">ファイル保存先FullPath</param>
        /// <returns>void</returns>
        Task WriteExtractTextAsync(IEnumerable<string> csvStringLists, string saveFileFullName);

        /// <summary>
        /// 非同期処理
        /// 指定テーブルのBlobClientから、ストリームを読み込み、テキストを抽出し、ファイル保存する処理の一列の作業タスク
        /// </summary>
        /// <param name="target">指定テーブル</param>
        /// <returns>指定テーブル, タスク処理結果</returns>
        Task<Tuple<string, bool>> ExtractTargetAsnc(string target);

        /// <summary>
        /// 同期処理
        /// 指定テーブルのBlobClientから、ストリームを読み込み、テキストを抽出し、ファイル保存する処理の一列の作業タスク
        /// </summary>
        /// <param name="target">指定テーブル</param>
        /// <returns>指定テーブル, タスク処理結果</returns>
        Task<Tuple<string, bool>> ExtractTarget(string target);
    }
}