using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;

namespace AzureStorageCSVFileExtract
{
    public static class ConfigValue
    {
        public static readonly string ConnetionString = ConfigurationManager.ConnectionStrings["AzureBlobStorageConnection"].ConnectionString ?? default;
        public static readonly string ContainerName = ConfigurationManager.AppSettings.Get("BlobContainerName") ?? default;
        public static readonly string BlobPrefixPath = ConfigurationManager.AppSettings.Get("BlobPrefixPath") ?? default;
        public static readonly string BlobFileExtName = ConfigurationManager.AppSettings.Get("BlobFileExtName") ?? default;
        public static readonly string BlobSaveFilePath = ConfigurationManager.AppSettings.Get("ExtractSaveFilePath") ?? default;
        public static readonly int ExtractCount = int.Parse(ConfigurationManager.AppSettings.Get("ExtractRecordCount"));
        public static readonly string LF = "\n";
        public static readonly string CR = "\r";

        public static T Get<T>(this NameValueCollection collection, string key, T defaultValue)
        {
            var value = collection[key];
            var converter = TypeDescriptor.GetConverter(typeof(T));
            if (string.IsNullOrWhiteSpace(value) || !converter.IsValid(value))
            {
                return defaultValue;
            }

            return (T)converter.ConvertFromInvariantString(value);
        }

        /// <summary>
        /// GetTargetBlobName
        /// </summary>
        /// <param name="target">テーブル名</param>
        /// <returns>BLOB Fullpath</returns>
        public static string GetTargetBlobName(string target)
        {
            return string.Join("/", ConfigValue.BlobPrefixPath, string.Concat(target, ConfigValue.BlobFileExtName));
        }

        /// <summary>
        /// GetSaveFileName
        /// </summary>
        /// <param name="target">テーブル名</param>
        /// <returns>ファイル保存先FullPath</returns>
        public static string GetSaveFileName(string target)
        {
            try
            {
                var blobSaveDirFullPath = Path.GetFullPath(ConfigValue.BlobSaveFilePath);

                if (!Directory.Exists(blobSaveDirFullPath))
                {
                    Directory.CreateDirectory(blobSaveDirFullPath);
                }

                var destPath = Path.Combine(blobSaveDirFullPath, Path.GetFileName(string.Concat(target, ConfigValue.BlobFileExtName)));
                return destPath;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool IsExtractTargetIncludeCR(string target)
        {
            var targetLists = ConfigurationManager.AppSettings.Get("ExtractTargetsIncludeCR") ?? string.Empty;
            var hasTarget = targetLists.Split(',', (char)StringSplitOptions.RemoveEmptyEntries);

            if (hasTarget.Contains(target))
            {
                return true;
            }
            return false;
        }

        public static bool IsExtractTargetREP(string target)
        {
            var targetLists = ConfigurationManager.AppSettings.Get("ExtractTargetsREP") ?? string.Empty;
            var hasTarget = targetLists.Split(',', (char)StringSplitOptions.RemoveEmptyEntries);

            if (hasTarget.Contains(target))
            {
                return true;
            }
            return false;
        }
    }
}
