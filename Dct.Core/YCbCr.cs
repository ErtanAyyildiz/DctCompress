namespace Dct.Core
{
    public sealed class YCbCr
    {
        public YCbCr()
        {
        }

        public YCbCr(double y, double cb, double cr)
        {
            Y = y;
            Cb = cb;
            Cr = cr;
        }

        public double Y { get; set; }
        public double Cb { get; set; }
        public double Cr { get; set; }
    }
}
