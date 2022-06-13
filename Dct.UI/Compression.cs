using Dct.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace Dct.UI
{
    public partial class Compression : Form
    {
        Bitmap uncompressedBitmap;
        Bitmap uncompressedSecondFrame;

        public Compression()
        {
            InitializeComponent();
        }

        void OpenUncompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.InitialDirectory = @"N:\My Documents\My Pictures";
            openFileDialog1.Filter = "JPEG Compressed Image (*.jpg|*.jpg" + "|GIF Image(*.gif|*.gif" + "|Bitmap Image(*.bmp|*.bmp";
            openFileDialog1.Multiselect = true;
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                uncompressedBitmap = new Bitmap(openFileDialog1.FileName);
                uncompressedBitmap.Save("OriginalImage.bmp", ImageFormat.Bmp);
            }

            pictureBox1.Image = uncompressedBitmap;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

        }

        void OpenCompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            byte[] fileBytes;
            // Set filter options and filter index.
            openFileDialog1.InitialDirectory = @"N:\My Documents\My Pictures";
            openFileDialog1.Filter = "My compression (*.cmpr|*.cmpr";
            openFileDialog1.Multiselect = false;
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                DCT dct = new DCT();
                dct.openSavedFile(openFileDialog1.FileName);

                Bitmap finalImage = GenerateRgbBitmapFromYCbCr(dct.YImage, dct.CbImage, dct.CrImage);
                finalImage.Save("FinalJPEG.bmp", ImageFormat.Bmp);
                pictureBox2.Image = finalImage;
                pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
                //uncompressedBitmap = new Bitmap(openFileDialog1.FileName);
            }
        }



        void CompressImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.Filter = "My compression (*.cmpr|*.cmpr";
            saveFileDialog1.FilterIndex = 1;
            //if (saveFileDialog1.ShowDialog() == DialogResult.OK) {

            //}
            CompressBitmap(uncompressedBitmap);
        }



        /*
            Compress an image bitmap using jpeg techniques
            ---------------------------Starting point of compression.-------------------------------------------------------
        */
        public void CompressBitmap(Bitmap uncompressed)
        {
            int width = uncompressed.Width;
            int height = uncompressed.Height;

            double[,] Y = new double[width, height];
            double[,] Cb = new double[width / 2, height / 2];
            double[,] Cr = new double[width / 2, height / 2];

            GenerateYcbcrBitmap(uncompressed, ref Y, ref Cb, ref Cr);
            SetYImage(Y, Cb, Cr);//Sets the images to display on screen

            Bitmap testBitmap = new Bitmap(width, height);
            testBitmap = GenerateRgbBitmapFromYCbCr(Y, Cb, Cr);
            testBitmap.Save("SubsampledImage.bmp", ImageFormat.Bmp);

            DCT dct = new DCT();
            dct.setY(Y);
            dct.setCb(Cb);
            dct.setCr(Cr);

            dct.runDCT();

            System.Windows.Forms.MessageBox.Show("Compression complete!");
        }
        //------------------------------------------------------------------------------------------------------------------


        /**
        Old code for saving to custom fileType.  not useable anymore.
            **/
        public void OldSavingStuff(double[,] Y)
        {
            int width = Y.GetLength(0);
            int height = Y.GetLength(1);
            YCbCr[,] ycbcrPixels = new YCbCr[width, height];

            Bitmap testBitmap = new Bitmap(width, height);
            //testBitmap = generateRgbBitmap(ycbcrPixels);

            testBitmap.Save("SubsampledImage.bmp", ImageFormat.Bmp);

            byte[] bytesToSave = new byte[width * height * 3 + 8];

            int byteOffset = 0;
            RedGreenBlue rgb = new RedGreenBlue();

            byte[] widthBytes = BitConverter.GetBytes(width);
            byte[] heightBytes = BitConverter.GetBytes(height);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(widthBytes);
                Array.Reverse(heightBytes);
            }

            //Saving image width to beginning of byte array.
            for (int i = 0; i < widthBytes.Length; i++)
            {
                bytesToSave[byteOffset++] = widthBytes[i];
            }

            //saving image Height to beginning of byte array.
            for (int i = 0; i < heightBytes.Length; i++)
            {
                bytesToSave[byteOffset++] = heightBytes[i];
            }

            //Saving image data to byte array.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    rgb = ConvertYCbCrToRgb(1, 2, 3);
                    bytesToSave[y * width + x + byteOffset++] = (byte)rgb.Red;
                    bytesToSave[y * width + x + byteOffset++] = (byte)rgb.Green;
                    bytesToSave[y * width + x + byteOffset] = (byte)rgb.Blue;
                    //Debug.WriteLine(uncompressed.GetPixel(x,y));
                    //Debug.WriteLine(testBitmap.GetPixel(x,y));
                }
            }
            File.WriteAllBytes("TestFile.cmpr", bytesToSave);
        }

        /*Converts a RGB bitmap to YCbCr*/
        void GenerateYcbcrBitmap(Bitmap uncompressed, ref double[,] Y, ref double[,] Cb, ref double[,] Cr)
        {
            YCbCr[,] ycbcrPixels = new YCbCr[uncompressed.Width, uncompressed.Height];
            Color pixel;
            RedGreenBlue rgb = new RedGreenBlue();

            for (int y = 0; y < ycbcrPixels.GetLength(1); y++)
            {
                for (int x = 0; x < ycbcrPixels.GetLength(0); x++)
                {
                    if (x / 2 >= Cb.GetLength(0) || y / 2 >= Cb.GetLength(1)) continue;
                    pixel = uncompressed.GetPixel(x, y);
                    rgb.Red = pixel.R;
                    rgb.Green = pixel.G;
                    rgb.Blue = pixel.B;
                    ycbcrPixels[x, y] = ConvertRgbToYCbCr(rgb);
                    Y[x, y] = ycbcrPixels[x, y].Y;
                    Cb[x / 2, y / 2] = ycbcrPixels[x, y].Cb;
                    Cr[x / 2, y / 2] = ycbcrPixels[x, y].Cr;
                }
            }
        }

        void SetYImage(double[,] Y, double[,] Cb, double[,] Cr)
        {
            int width = Y.GetLength(0);
            int height = Y.GetLength(1);

            Bitmap bitY, bitCb, bitCr;
            bitY = new Bitmap(width, height);
            bitCb = new Bitmap(width / 2, height / 2);
            bitCr = new Bitmap(width / 2, height / 2);

            Color color = new Color();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //color = Color.FromArgb((int)pixels[x, y].getY(), (int)pixels[x, y].getY(), (int)pixels[x, y].getY());
                    color = Color.FromArgb((int)Y[x, y], (int)Y[x, y], (int)Y[x, y]);
                    bitY.SetPixel(x, y, color);
                }
            }

            for (int y = 0; y < bitCb.Height; y++)
            {
                for (int x = 0; x < bitCb.Width; x++)
                {
                    //color = Color.FromArgb((int)pixels[x,y].getCb(), (int)pixels[x, y].getCb(), (int)pixels[x, y].getCb());
                    color = Color.FromArgb((int)Cb[x, y], (int)Cb[x, y], (int)Cb[x, y]);
                    bitCb.SetPixel(x, y, color);
                    //color = Color.FromArgb((int)pixels[x,y].getCr(), (int)pixels[x, y].getCr(), (int)pixels[x, y].getCr());
                    color = Color.FromArgb((int)Cr[x, y], (int)Cr[x, y], (int)Cr[x, y]);
                    bitCr.SetPixel(x, y, color);
                }
            }
            pictureBox3.Image = bitY;
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            bitCb.Save("CbImage.bmp", ImageFormat.Bmp);
            pictureBox4.Image = bitCb;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox5.Image = bitCr;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
        }

        /*convert YCbCr values to a RGB bitmap*/
        Bitmap GenerateRgbBitmapFromYCbCr(double[,] Y, double[,] Cb, double[,] Cr)
        {
            int width = Y.GetLength(0);
            int height = Y.GetLength(1);
            Bitmap bitmap = new Bitmap(width, height);
            Color color;
            RedGreenBlue rgb = new RedGreenBlue();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (x / 2 >= Cb.GetLength(0) || y / 2 >= Cb.GetLength(1)) continue;
                    rgb = ConvertYCbCrToRgb(Y[x, y], Cb[x / 2, y / 2], Cr[x / 2, y / 2]);
                    color = Color.FromArgb(rgb.Red, rgb.Green, rgb.Blue);
                    bitmap.SetPixel(x, y, color);
                }
            }
            return bitmap;
        }

        /*Converts RGB pixel to YCbCr*/
        YCbCr ConvertRgbToYCbCr(RedGreenBlue rgb)
        {
            YCbCr output = new YCbCr();
            output.Y= 16 + (rgb.Red * 0.257 + rgb.Green * 0.504 + rgb.Blue * 0.098);
            output.Cb = 128 + (rgb.Red * -0.148 + rgb.Green * -0.291 + rgb.Blue * 0.439);
            output.Cr = 128 + (rgb.Red * 0.439 + rgb.Green * -0.368 + rgb.Blue * -0.071);
            return output;
        }

        /*Converts YCbCr to RGB*/
        RedGreenBlue ConvertYCbCrToRgb(double curY, double curCb, double curCr)
        {
            RedGreenBlue output = new RedGreenBlue();
            double red = (curY - 16) * 1.164 + (curCb - 128) * 0 + (curCr - 128) * 1.596;
            output.Red = (short)Math.Round(red);
            double green = (curY - 16) * 1.164 + (curCb - 128) * -0.392 + (curCr - 128) * -0.813;
            output.Green = (short)green;
            double blue = (curY - 16) * 1.164 + (curCb - 128) * 2.017 + (curCr - 128) * 0;
            output.Blue = (short)blue;
            return output;
        }


        void LoadSecondFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            // Set filter options and filter index.
            openFileDialog1.InitialDirectory = @"N:\My Documents\My Pictures";
            openFileDialog1.Filter = "JPEG Compressed Image (*.jpg|*.jpg" + "|GIF Image(*.gif|*.gif" + "|Bitmap Image(*.bmp|*.bmp";
            openFileDialog1.Multiselect = true;
            openFileDialog1.FilterIndex = 1;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                uncompressedSecondFrame = new Bitmap(openFileDialog1.FileName);
                //uncompressedSecondFrame.Save("OriginalSecondImage.bmp", ImageFormat.Bmp);
            }

            pictureBox6.Image = uncompressedSecondFrame;
            pictureBox6.SizeMode = PictureBoxSizeMode.Zoom;
        }

        void SaveCompressedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog() { FileName = "compressed.jpeg" };
            DialogResult dialog = save.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                pictureBox2.Image.Save(save.FileName, ImageFormat.Jpeg);
            }
        }

        /*Generate motion vectors between the two frames*/
        void GenerateMotionVectorsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int width = uncompressedSecondFrame.Width;
            int height = uncompressedSecondFrame.Height;

            double[,] Y = new double[width, height];
            double[,] Cb = new double[width / 2, height / 2];
            double[,] Cr = new double[width / 2, height / 2];

            //generate the Y, Cb, Cr values from the second frame
            GenerateYcbcrBitmap(uncompressedSecondFrame, ref Y, ref Cb, ref Cr);

            double[,] Y2 = new double[width, height];
            double[,] Cb2 = new double[width / 2, height / 2];
            double[,] Cr2 = new double[width / 2, height / 2];

            GenerateYcbcrBitmap(uncompressedBitmap, ref Y2, ref Cb2, ref Cr2);

            DCT dct = new DCT();
            Point[,] vectors = new Point[(int)Math.Ceiling((double)width / 8), (int)Math.Ceiling((double)height / 8)];
            Block[,] YerrorBlocks = new Block[(int)Math.Ceiling((double)width / 8), (int)Math.Ceiling((double)height / 8)];
            Block[,] CberrorBlocks = new Block[(int)Math.Ceiling((double)width / 8), (int)Math.Ceiling((double)height / 8)];
            Block[,] CrerrorBlocks = new Block[(int)Math.Ceiling((double)width / 8), (int)Math.Ceiling((double)height / 8)];
            VideoCompression vidcom = new VideoCompression();

            for (int y = 0; y < height; y += 8)
            {
                for (int x = 0; x < width; x += 8)
                {
                    Block Yblock = dct.generateBlock(Y, x, y);
                    Block Cbblock = dct.generateBlock(Cb, x, y);
                    Block Crblock = dct.generateBlock(Cr, x, y);

                    vectors[x / 8, y / 8] = vidcom.getVector(Y2, Yblock, x, y, 15);
                    vidcom.getErrorForPosition(Cb2, Cbblock, vectors[x / 8, y / 8].X, vectors[x / 8, y / 8].Y, 2);
                    vidcom.getErrorForPosition(Cr2, Crblock, vectors[x / 8, y / 8].X, vectors[x / 8, y / 8].Y, 3);
                    YerrorBlocks[x / 8, y / 8] = vidcom.getCurrentYErrorBlock();
                    CberrorBlocks[x / 8, y / 8] = vidcom.getCurrentCbErrorBlock();
                    CrerrorBlocks[x / 8, y / 8] = vidcom.getCurrentCrErrorBlock();

                    Debug.Write("(" + vectors[x / 8, y / 8].X + "," + vectors[x / 8, y / 8].Y + "),");
                }
                Debug.WriteLine("");
            }


            dct.compressPframe(YerrorBlocks, CberrorBlocks, CrerrorBlocks, vectors, width, height);


            Block[,] finalY = new Block[dct.Yblocks.GetLength(0), dct.Yblocks.GetLength(1)];
            Block[,] finalCb = new Block[dct.Yblocks.GetLength(0), dct.Yblocks.GetLength(1)];
            Block[,] finalCr = new Block[dct.Yblocks.GetLength(0), dct.Yblocks.GetLength(1)];

            for (int y = 0; y < dct.Yblocks.GetLength(0); y++)
            {
                for (int x = 0; x < dct.Yblocks.GetLength(1); x++)
                {
                    vidcom.getOriginalFromError(Y2, dct.Yblocks[x, y], x * 8 + vectors[x, y].X, y * 8 + vectors[x, y].Y, 1);
                    finalY[x, y] = vidcom.YpostBlock;
                    vidcom.getOriginalFromError(Cb2, dct.Cbblocks[x, y], x * 8 + vectors[x, y].X, y * 8 + vectors[x, y].Y, 2);
                    finalCb[x, y] = vidcom.CbpostBlock;
                    vidcom.getOriginalFromError(Cr2, dct.Crblocks[x, y], x * 8 + vectors[x, y].X, y * 8 + vectors[x, y].Y, 3);
                    finalCr[x, y] = vidcom.CrpostBlock;
                }
            }

            double[,] finalYimage = dct.makeDoubleArrayFromBlocks(finalY, width, height);
            double[,] finalCbimage = dct.makeDoubleArrayFromBlocks(finalCb, width, height);
            double[,] finalCrimage = dct.makeDoubleArrayFromBlocks(finalCr, width, height);

            Bitmap pframe = GenerateRgbBitmapFromYCbCr(finalYimage, finalCbimage, finalCrimage);
            pictureBox2.Image = pframe;
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
        }
    }
}
