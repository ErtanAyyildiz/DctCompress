using System;
using System.IO;

namespace Dct.Core
{
    public static class FileFunctions
    {
        /// <summary>
        /// Sıkıştırılmış dosyayı kaydet.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public static void SaveCompressed(int[] data, int width, int height)
        {
            int byteOffset = 0;
            byte[] totalSize = new byte[data.Length + 8];
            byte[] wBytes = BitConverter.GetBytes(width);
            byte[] hBytes = BitConverter.GetBytes(height);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(wBytes);
                Array.Reverse(hBytes);
            }

            //Saving image width to beginning of byte array.
            for (var i = 0; i < wBytes.Length; i++)
            {
                totalSize[byteOffset++] = wBytes[i];
            }

            //saving image Height to beginning of byte array.
            for (var i = 0; i < hBytes.Length; i++)
            {
                totalSize[byteOffset++] = hBytes[i];
            }

            for (var i = 0; i < data.Length; i++)
            {
                var currentData = (sbyte)data[i];
                totalSize[i + byteOffset] = (byte)currentData;
            }

            File.WriteAllBytes("TestFile.cmpr", totalSize);
        }

        /// <summary>
        /// Sıkıştırılmış dosyayı aç!
        /// </summary>
        /// <param name="filename">Dosya yolu...</param>
        /// <returns></returns>
        public static byte[] OpenCompressed(string filename) => File.ReadAllBytes(filename);
    }
}
