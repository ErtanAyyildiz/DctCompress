using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Dct.Core
{
    public class DiscreteCosTransform : ICompressor
    {
        public Block[,] Yblocks, Cbblocks, Crblocks;
        private double[,] yImage, crImage, cbImage;

        readonly int[,] luminance = {
            { 16, 11, 10, 16, 24, 40, 51, 61 },
            { 12, 12, 14, 19, 26, 58, 60, 55 },
            { 14, 13, 16, 24, 40, 57, 69, 56 },
            { 14, 17, 22, 29, 51, 87, 80, 62 },
            { 18, 22, 37, 56, 68, 109, 103, 77 },
            { 24, 35, 55, 64, 81, 104, 113, 92 },
            { 49, 64, 78, 87, 103, 121, 120, 101 },
            { 72, 92, 95, 98, 112, 100, 103, 99 }};

        readonly int[,] chrominance = {
            { 17, 18, 24, 27, 47, 99, 99, 99 },
            { 18, 21, 26, 66, 99, 99, 99, 99 },
            { 24, 26, 56, 99, 99, 99, 99, 99 },
            { 47, 66, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 },
            { 99, 99, 99, 99, 99, 99, 99, 99 }};

        public double[,] YImage { get => yImage; set => yImage = value; }
        public double[,] CrImage { get => crImage; set => crImage = value; }
        public double[,] CbImage { get => cbImage; set => cbImage = value; }

        public Block GenerateBlock(double[,] fullSize, int xPosition, int yPosition)
        {
            Block block = new Block();

            for (int y = yPosition; y < yPosition + 8; y++)
            {
                for (int x = xPosition; x < xPosition + 8; x++)
                {
                    if (x < fullSize.GetLength(0) && y < fullSize.GetLength(1))
                    {
                        block[x - xPosition, y - yPosition] = fullSize[x, y];
                    }
                    else
                    {
                        block[x - xPosition, y - yPosition] = 0.0;
                    }
                }
            }
            return block;
        }

        public void Compress()
        {
            int horizontalBlocks = (int)Math.Ceiling((double)YImage.GetLength(0) / 8);//amount of full 8x8 blocks will fit horizontally
            int verticalBlocks = (int)Math.Ceiling((double)YImage.GetLength(1) / 8);//amount of full 8x8 blocks will fit vertically

            Block[,] Yblocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] YdctBlocks = new Block[horizontalBlocks, verticalBlocks];


            Block[,] Cbblocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CbdctBlocks = new Block[horizontalBlocks, verticalBlocks];


            Block[,] Crblocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CrdctBlocks = new Block[horizontalBlocks, verticalBlocks];


            List<int> YSaveBuffer = new List<int>();
            List<int> CbSaveBuffer = new List<int>();
            List<int> CrSaveBuffer = new List<int>();
            int toSaveBufferPos = 0;

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    Yblocks[x, y] = GenerateBlock(YImage, x * 8, y * 8);//which block, multiplied by block offset (8) 
                    YdctBlocks[x, y] = new Block();

                    Cbblocks[x, y] = GenerateBlock(CbImage, x * 8, y * 8);//which block, multiplied by block offset (8) 
                    CbdctBlocks[x, y] = new Block();

                    Crblocks[x, y] = GenerateBlock(CrImage, x * 8, y * 8);//which block, multiplied by block offset (8) 
                    CrdctBlocks[x, y] = new Block();

                }
            }

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    //Debug.WriteLine("-----------Starting------------");
                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {

                            YdctBlocks[x, y][u, v] = ApplyDCTFormula(Yblocks[x, y], u, v);
                            CbdctBlocks[x, y][u, v] = ApplyDCTFormula(Cbblocks[x, y], u, v);
                            CrdctBlocks[x, y][u, v] = ApplyDCTFormula(Crblocks[x, y], u, v);

                        }
                        //Debug.WriteLine("");
                    }

                    YdctBlocks[x, y] = ApplyQuantization(YdctBlocks[x, y], luminance);
                    CbdctBlocks[x, y] = ApplyQuantization(CbdctBlocks[x, y], chrominance);
                    CrdctBlocks[x, y] = ApplyQuantization(CrdctBlocks[x, y], chrominance);

                    int[] Yzig = ApplyZigZag(YdctBlocks[x, y]);


                    int[] Yencoded = RunLengthEncode(Yzig);
                    YSaveBuffer.Add(Yencoded.Length);
                    for (int i = 0; i < Yencoded.Length; i++)
                    {
                        YSaveBuffer.Add(Yencoded[i]);
                    }

                    int[] Cbzig = new int[128];
                    int[] Crzig = new int[128];

                    int[] Cbencoded = new int[128];
                    int[] Crencoded = new int[128];


                    Cbzig = ApplyZigZag(CbdctBlocks[x, y]);
                    Crzig = ApplyZigZag(CrdctBlocks[x, y]);

                    Cbencoded = RunLengthEncode(Cbzig);
                    Crencoded = RunLengthEncode(Crzig);

                    //first save the length of the run length, so we know how far to read later
                    CbSaveBuffer.Add(Cbencoded.Length);
                    for (int i = 0; i < Cbencoded.Length; i++)
                    {
                        CbSaveBuffer.Add(Cbencoded[i]);
                    }
                    CrSaveBuffer.Add(Crencoded.Length);
                    for (int i = 0; i < Crencoded.Length; i++)
                    {
                        CrSaveBuffer.Add(Crencoded[i]);
                    }
                }//x
            }//y

            Debug.WriteLine("File length = " + toSaveBufferPos);

            int position = 0;
            int totalCount = YSaveBuffer.Count + CbSaveBuffer.Count + CrSaveBuffer.Count;
            int[] toSave = new int[totalCount + 2];
            for (int i = 0; i < YSaveBuffer.Count; i++)
            {
                toSave[position++] = YSaveBuffer[i];
            }
            toSave[position++] = 127;
            for (int i = 0; i < CbSaveBuffer.Count; i++)
            {
                toSave[position++] = CbSaveBuffer[i];
            }
            toSave[position++] = 127;
            for (int i = 0; i < CrSaveBuffer.Count; i++)
            {
                toSave[position++] = CrSaveBuffer[i];
            }

            FileFunctions.Save(toSave, YImage.GetLength(0), YImage.GetLength(1));
            byte[] savedData = FileFunctions.OpenCompressed(FileFunctions.FILE_NAME);
            DecodeSaveArray(savedData);
        }


        /*
        Generates an 8x8 block from a position 
            */

        /*
        Puts an array of blocks back together as a single double array, ready for conversion to image
            */
        public double[,] MakeDoubleArrayFromBlocks(Block[,] blocks, int width, int height)
        {
            double[,] image = new double[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int XblockPosition = x / 8;
                    int YblockPosition = y / 8;
                    int XinsidePosition = x - (XblockPosition * 8);
                    int YinsidePosition = y - (YblockPosition * 8);

                    image[x, y] = blocks[XblockPosition, YblockPosition][XinsidePosition, YinsidePosition];
                }
            }

            return image;
        }

        /*
        Applies DCT formula to a block, and returns the post DCT pixel
            */
        public double ApplyDCTFormula(Block input, double u, double v)
        {
            double sum = 0, firstCos, secondCos;

            for (double i = 0; i < 8; i++)
            {
                for (double j = 0; j < 8; j++)
                {
                    firstCos = Math.Cos((2 * i + 1) * u * Math.PI / 16);
                    secondCos = Math.Cos((2 * j + 1) * v * Math.PI / 16);
                    sum += firstCos * secondCos * input[(int)i, (int)j];
                    //if (i < 4 && j < 4) sum += firstCos * secondCos * input.get((int)i, (int)j);
                }
            }

            sum *= c(u) * c(v) / 4;

            return sum;
        }

        /*
        Applies inverse DCT formula and returns the image pixel
            */
        public double ApplyIDCTFormula(Block input, double i, double j)
        {
            double sum = 0, firstCos, secondCos;

            for (double u = 0; u < 8; u++)
            {
                for (double v = 0; v < 8; v++)
                {
                    firstCos = Math.Cos((2 * i + 1) * u * Math.PI / 16);
                    secondCos = Math.Cos((2 * j + 1) * v * Math.PI / 16);
                    sum += c(u) * c(v) / 4 * firstCos * secondCos * input[(int)u, (int)v];
                }
            }

            return sum;
        }

        /*
        C() function as defined in DCT
            */
        public double c(double input)
        {
            if (input == 0) return 1 / Math.Sqrt(2);
            return 1;
        }

        /*
        Creates a bitmap from an array of 8*8 blocks.
            */
       

        /*
        Divides the values of the block by a passed quantization table, and returns the outcome
            */
        public Block ApplyQuantization(Block block, int[,] table)
        {
            Block quantizedBlock = new Block();
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    quantizedBlock[x, y] = Math.Round((double)block[x, y] / table[x, y]);
                }
            }

            return quantizedBlock;
        }

        /*
        Multiplies the values of a quantized block by a quantization table to undo quantization. returns the outcome
            */
        public Block RemoveQuantization(Block block, int[,] table)
        {
            var buffer = new Block();
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    buffer[x, y] = block[x, y] * table[x, y];
                }
            }
            return buffer;
        }

        /*
        converts a block into a 1 dimensional array
            */
        public int[] ApplyZigZag(Block block)
        {
            int[] zigzag = new int[64];
            int arrayPos = 0;
            int yMove = -1;
            int xMove = 1;
            int x = 0;
            int y = 0;

            zigzag[arrayPos++] = (int)block[x, y];

            for (int level = 0; level < 8; level++)
            {

                for (int depth = 0; depth < level; depth++)
                {
                    x += xMove;
                    y += yMove;

                    zigzag[arrayPos++] = (int)block[x, y];
                }

                if (level == 7) break;

                if (xMove < 0) y++; else x++;

                zigzag[arrayPos++] = (int)block[x, y];

                yMove *= -1;
                xMove *= -1;
            }

            yMove *= -1;
            xMove *= -1;

            if (xMove < 0) y++; else x++;

            zigzag[arrayPos++] = (int)block[x, y];

            for (int level = 6; level > 0; level--)
            {

                for (int depth = level; depth > 0; depth--)
                {
                    x += xMove;
                    y += yMove;

                    zigzag[arrayPos++] = (int)block[x, y];
                }

                if (xMove < 0) x++; else y++;

                zigzag[arrayPos++] = (int)block[x, y];

                yMove *= -1;
                xMove *= -1;
            }

            return zigzag;
        }

        /*
        undoes zig zag on an int array, and outputs the original 8x8 block
            */
        public Block UndoZigZag(int[] array)
        {
            Block block = new Block();

            int arrayPos = 0;
            int yMove = -1;
            int xMove = 1;
            int x = 0;
            int y = 0;

            block[x, y] = array[arrayPos++];

            for (int level = 0; level < 8; level++)
            {

                for (int depth = 0; depth < level; depth++)
                {
                    x += xMove;
                    y += yMove;

                    block[x, y] = array[arrayPos++];
                }

                if (level == 7) break;

                if (xMove < 0) y++; else x++;

                block[x, y] = array[arrayPos++];

                yMove *= -1;
                xMove *= -1;
            }

            yMove *= -1;
            xMove *= -1;

            if (xMove < 0) y++; else x++;

            block[x, y] = array[arrayPos++];

            for (int level = 6; level > 0; level--)
            {

                for (int depth = level; depth > 0; depth--)
                {
                    x += xMove;
                    y += yMove;

                    block[x, y] = array[arrayPos++];
                }

                if (xMove < 0) x++; else y++;

                block[x, y] = array[arrayPos++];

                yMove *= -1;
                xMove *= -1;
            }


            return block;
        }

        /*
        Run length encodes an int array. uses 255 as the key, as none of the quantized values get that high.
            */
        public int[] RunLengthEncode(int[] array)
        {
            int[] buffer = new int[256];
            int pos = 0;
            int count = 1;
            int currentValue = array[0];

            for (int i = 1; i < array.GetLength(0); i++)
            {
                if (array[i] != currentValue)
                {
                    if (count < 4)
                    {
                        for (int j = 0; j < count; j++)
                        {
                            buffer[pos++] = currentValue;
                        }
                        count = 1;
                        currentValue = array[i];
                        continue;
                    }
                    buffer[pos++] = 127;
                    buffer[pos++] = count;
                    buffer[pos++] = currentValue;
                    count = 1;
                    currentValue = array[i];
                    continue;
                }
                count++;
            }

            int[] output = new int[pos];

            for (int i = 0; i < pos; i++)
            {
                output[i] = buffer[i];
            }

            return output;
        }

        /*
        undoes run length encoding, extending the compressed array
            */
        public int[] UndoRunlengthEncoding(int[] array)
        {
            int[] output = new int[64];
            int pos = 0;
            int count;

            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == 127)
                {
                    i++;
                    count = array[i++];
                    for (int j = 0; j < count; j++)
                    {
                        output[pos++] = array[i];
                    }
                    continue;
                }
                output[pos++] = array[i];
            }

            while (pos < 63)
            {
                output[pos++] = 0;
            }

            return output;
        }

        /*
        Decodes an array of saved data
            */
        public void DecodeSaveArray(byte[] byteData)
        {
            int[] data = new int[byteData.Length];

            for (int i = 0; i < byteData.Length; i++)
            {
                data[i] = (sbyte)byteData[i];
            }

            int currentRunType = 1;
            int currentCount = 0;
            List<int> currentRun;

            List<List<int>> Yencoded = new List<List<int>>();

            List<List<int>> Cbencoded = new List<List<int>>();

            List<List<int>> Crencoded = new List<List<int>>();

            byte[] widthBytes = new byte[4];
            byte[] heightBytes = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                widthBytes[i] = byteData[i];
                heightBytes[i] = byteData[i + 4];
            }

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(widthBytes);
                Array.Reverse(heightBytes);
            }

            int width = BitConverter.ToInt32(widthBytes, 0);
            Debug.WriteLine("width:" + width);
            int height = BitConverter.ToInt32(heightBytes, 0);
            Debug.WriteLine("height:" + height);

            for (int i = 8; i < data.Length; i++)
            {
                if (currentRunType == 1)//if its a Y run
                {
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Yencoded.Add(currentRun);
                    if (data[i + 1] == 127)
                    {
                        currentRunType++;
                        i++;
                    }
                }
                else if (currentRunType == 2)
                {//if Cb run
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Cbencoded.Add(currentRun);
                    if (data[i + 1] == 127)
                    {
                        currentRunType++;
                        i++;
                    }
                }
                else if (currentRunType == 3)
                {//if Cr run
                    currentCount = data[i];
                    currentRun = new List<int>();
                    for (int j = 0; j < currentCount; j++)
                    {
                        i++;
                        currentRun.Add(data[i]);
                    }
                    Crencoded.Add(currentRun);
                    //currentRunType = 1;
                }
            }

            Decompress(Yencoded, Cbencoded, Crencoded, width, height);
        }

        /*
        Undo each of the compression steps
            */
        public void Decompress(List<List<int>> YencodedList, List<List<int>> CbencodedList, List<List<int>> CrencodedList, int width, int height)
        {
            int horizontalBlocks = (int)Math.Ceiling((double)width / 8);//amount of full 8x8 blocks will fit horizontally
            int verticalBlocks = (int)Math.Ceiling((double)height / 8);//amount of full 8x8 blocks will fit vertically

            Block[,] YpostBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CbpostBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CrpostBlocks = new Block[horizontalBlocks, verticalBlocks];

            Block[,] YdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CbdctBlocks = new Block[horizontalBlocks, verticalBlocks];
            Block[,] CrdctBlocks = new Block[horizontalBlocks, verticalBlocks];

            int[] Yencoded, Cbencoded, Crencoded;

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    YpostBlocks[x, y] = new Block();
                    CbpostBlocks[x, y] = new Block();
                    CrpostBlocks[x, y] = new Block();
                }
            }

            int[] Yzig, Cbzig, Crzig;

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    Yencoded = Convert2dListToArray(YencodedList, verticalBlocks * y + x);
                    Cbencoded = Convert2dListToArray(CbencodedList, verticalBlocks * y + x);
                    Crencoded = Convert2dListToArray(CrencodedList, verticalBlocks * y + x);

                    Yzig = UndoRunlengthEncoding(Yencoded);
                    Cbzig = UndoRunlengthEncoding(Cbencoded);
                    Crzig = UndoRunlengthEncoding(Crencoded);

                    YdctBlocks[x, y] = UndoZigZag(Yzig);
                    CbdctBlocks[x, y] = UndoZigZag(Cbzig);
                    CrdctBlocks[x, y] = UndoZigZag(Crzig);
                }
            }

            for (int y = 0; y < verticalBlocks; y++)
            {
                for (int x = 0; x < horizontalBlocks; x++)
                {
                    YdctBlocks[x, y] = RemoveQuantization(YdctBlocks[x, y], luminance);
                    CbdctBlocks[x, y] = RemoveQuantization(CbdctBlocks[x, y], chrominance);
                    CrdctBlocks[x, y] = RemoveQuantization(CrdctBlocks[x, y], chrominance);

                    for (int v = 0; v < 8; v++)
                    {
                        for (int u = 0; u < 8; u++)
                        {
                            YpostBlocks[x, y][u, v] = ApplyIDCTFormula(YdctBlocks[x, y], u, v);

                            //if (x % 2 == 0 && y % 2 == 0)
                            //{
                            CbpostBlocks[x, y][u, v] = ApplyIDCTFormula(CbdctBlocks[x, y], u, v);
                            CrpostBlocks[x, y][u, v] = ApplyIDCTFormula(CrdctBlocks[x, y], u, v);
                            //}
                        }
                    }
                }
            }

            Yblocks = YpostBlocks;
            Cbblocks = CbpostBlocks;
            Crblocks = CrpostBlocks;

            YImage = MakeDoubleArrayFromBlocks(YpostBlocks, width, height);
            CbImage = MakeDoubleArrayFromBlocks(CbpostBlocks, width, height);
            CrImage = MakeDoubleArrayFromBlocks(CrpostBlocks, width, height);
        }

        /*
        Converts my 2d lists to 1d arrays
            */
        public int[] Convert2dListToArray(List<List<int>> list, int pos)
        {
            List<int> innerList = list[pos];
            int[] array = new int[innerList.Count];

            for (int i = 0; i < innerList.Count; i++)
            {
                array[i] = innerList[i];
            }

            return array;
        }


        /*
        compresses a pframe
            */
        public void OpenSavedFile(string filename)
        {
            byte[] savedData = FileFunctions.OpenCompressed(filename);
            DecodeSaveArray(savedData);
        }

        /*
        Decodes an array of saved data
            */
    }
}
