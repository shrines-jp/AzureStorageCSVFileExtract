using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageCSVFileExtract
{
    public static class StreamReaderExtention
    {
        public static string ReadLineWithDelimiter(this StreamReader sr, string delimiter)
        {
            var sb = new StringBuilder();
            var buf = new char[1];
            var chars = new StringBuilder();
            var isLineEnd = false;

            do
            {
                sr.Read(buf, 0, 1);
                sb.Append(buf);
                chars.Append(buf);

                if (!delimiter.StartsWith(chars.ToString()))
                {
                    chars.Clear();
                }
                else
                {
                    if (delimiter == chars.ToString()) isLineEnd = true;
                }

            } while (!sr.EndOfStream && !isLineEnd);

            if (isLineEnd)
            {
                sb = sb.Remove(sb.Length - delimiter.Length, delimiter.Length);
            }

            return sb.ToString();
        }

        public static async Task<string> ReadLineWithDelimiterAsync(this StreamReader sr, string delimiter)
        {
            var sb = new StringBuilder();
            var buf = new char[1];
            var chars = new StringBuilder();
            var isLineEnd = false;

            do
            {
                await sr.ReadAsync(buf, 0, 1);
                sb.Append(buf);
                chars.Append(buf);

                if (!delimiter.StartsWith(chars.ToString()))
                {
                    chars.Clear();
                }
                else
                {
                    if (delimiter == chars.ToString()) isLineEnd = true;
                }

            } while (!sr.EndOfStream && !isLineEnd);

            if (isLineEnd)
            {
                sb = sb.Remove(sb.Length - delimiter.Length, delimiter.Length);
            }

            return sb.ToString();
        }
    }
}
