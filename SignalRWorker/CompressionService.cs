using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace SignalRWorker
{
    public static class CompressionService
    {
        public static byte[] Compress(string data)
        {
            if (string.IsNullOrEmpty(data))
                return Array.Empty<byte>();

            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                {
                    var bytes = Encoding.UTF8.GetBytes(data);
                    gzipStream.Write(bytes, 0, bytes.Length);
                }

                return memoryStream.ToArray();
            }
        }

        public static string Decompress(byte[] compressedData)
        {
            if (compressedData == null || compressedData.Length == 0)
                return string.Empty;

            using (var memoryStream = new MemoryStream(compressedData))
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
