﻿using Dct.Core;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Dct.UI
{
    public partial class FormCompression : Form
    {
        Bitmap uncompressed;
        public CompressionStrategy CompressionStrategy { get; } =
            new CompressionStrategy(
                new DiscreteCosTransform()

                );
        public FormCompression()
        {
            InitializeComponent();
        }

        void BtnOpenOriginal_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                InitialDirectory = @"N:\My Documents\My Pictures",
                Filter = "Resim dosyaları (*.jpg|*.jpg" + "|GIF Image(*.gif|*.gif" + "|Bitmap Image(*.bmp|*.bmp",
            };

            if (open.ShowDialog().Equals(DialogResult.OK))
            {
                uncompressed = new Bitmap(open.FileName);
                pbOriginal.Image = uncompressed;
            }
        }

        void BtnCompress_Click(object sender, EventArgs e)
        {
            CompressBitmap(uncompressed);

        }

    
        YCbCr ConvertRgbToYCbCr(RedGreenBlue rgb)
        {
            YCbCr output = new YCbCr();
            output.Y = 16 + (rgb.Red * 0.257 + rgb.Green * 0.504 + rgb.Blue * 0.098);
            output.Cb = 128 + (rgb.Red * -0.148 + rgb.Green * -0.291 + rgb.Blue * 0.439);
            output.Cr = 128 + (rgb.Red * 0.439 + rgb.Green * -0.368 + rgb.Blue * -0.071);
            return output;
        }

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

        public void CompressBitmap(Bitmap uncompressed)
        {
            int width = uncompressed.Width;
            int height = uncompressed.Height;

            double[,] y = new double[width, height];
            double[,] cb = new double[width / 2, height / 2];
            double[,] cr = new double[width / 2, height / 2];

            GenerateYcbcrBitmap(
                uncompressed,
                ref y,
                ref cb,
                ref cr);

            SetYImage(y, cb, cr);

            Bitmap bmp = GenerateRgbBitmapFromYCbCr(y, cb, cr);
            bmp.Save("SubsampledImage.bmp", ImageFormat.Bmp);

            CompressionStrategy.YImage = y;
            CompressionStrategy.CrImage = cr;
            CompressionStrategy.CbImage = cb;

            Stopwatch sw = new Stopwatch();
            sw.Start();

            if (cbMultiThread.Checked)
            {
                CompressionStrategy.CompressAsync();
            }
            else
            {
                CompressionStrategy.Compress();
            }

            long duration = sw.ElapsedMilliseconds;

            string message = string.Format("Sıkıştırma operasyonu tamamlandı...\n" +
                "{0} süre: {1} ms.",
                 cbMultiThread.Checked ? "Multi-thread" : String.Empty,
                 duration);

            MessageBox.Show(
                message,
                "Dikkat!",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
                );
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
            bitY.Save("CyImage.bmp", ImageFormat.Bmp);

            pictureBox4.Image = bitCb;
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            bitCb.Save("CbImage.bmp", ImageFormat.Bmp);

            pictureBox5.Image = bitCr;
            pictureBox5.SizeMode = PictureBoxSizeMode.Zoom;
            bitCr.Save("CrImage.bmp", ImageFormat.Bmp);
        }


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

        void BtnOpenCompressed_Click(object sender, EventArgs e)
        {
            var asm = Assembly.GetExecutingAssembly();
            string path = Path.GetDirectoryName(asm.Location);

            var ofd = new OpenFileDialog
            {
                InitialDirectory = path,
                Filter = "Özel Sıkıştırılmış dosya! (*.cmpr|*.cmpr",
            };

            if (ofd.ShowDialog().Equals(DialogResult.OK))
            {
                CompressionStrategy.OpenSavedFile(ofd.FileName);
                Bitmap image = GenerateRgbBitmapFromYCbCr(
                    CompressionStrategy.YImage,
                    CompressionStrategy.CbImage,
                    CompressionStrategy.CrImage
                    );

                var formUncompress = new FormUncompress(image);
                formUncompress.ShowDialog();
            }
        }

        void BtnAbout_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        void CbMultiThread_CheckedChanged(object sender, EventArgs e)
        {
            CompressionStrategy.SetMultiThreadOperations(cbMultiThread.Checked);
        }
    }
}
