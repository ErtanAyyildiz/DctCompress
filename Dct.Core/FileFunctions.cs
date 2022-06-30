using System;
using System.IO;

namespace Dct.Core
{
    public static class FileFunctions
    {
        public const string FILE_NAME = "compressed.cmpr";
        /// <summary>
        /// Sıkıştırılmış dosyayı kaydet.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void Save(int[] data, int width, int height)
        {
            int offset = 0;
            byte[] totalSize = new byte[data.Length + 8];
            byte[] wBytes = BitConverter.GetBytes(width);
            byte[] hBytes = BitConverter.GetBytes(height);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(wBytes);
                Array.Reverse(hBytes);
            }


            for (var i = 0; i < wBytes.Length; i++)
            {
                totalSize[offset++] = wBytes[i];
            }


            for (var i = 0; i < hBytes.Length; i++)
            {
                totalSize[offset++] = hBytes[i];
            }

            for (var i = 0; i < data.Length; i++)
            {
                var currentData = (sbyte)data[i];
                totalSize[i + offset] = (byte)currentData;
            }

            File.WriteAllBytes(FILE_NAME, totalSize);
        }

        /// <summary>
        /// Sıkıştırılmış dosyayı aç!
        /// </summary>
        /// <param name="filename">Dosya yolu...</param>
        /// <returns></returns>
        public static byte[] OpenCompressed(string filename) => File.ReadAllBytes(filename);
    }
}
