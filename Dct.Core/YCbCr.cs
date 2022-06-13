namespace Dct.Core
{
    public sealed class YCbCr
    {
        private double Y;
        public double Cb;
        private double Cr;

        public double getY()
        {
            return Y;
        }

        public double getCb()
        {
            return Cb;
        }

        public double getCr()
        {
            return Cr;
        }

        public void setY(double Y)
        {

            this.Y = Y;
        }

        public void setCb(double Cb)
        {
            this.Cb = Cb;
        }

        public void setCr(double Cr)
        {
            this.Cr = Cr;
        }
    }
}
