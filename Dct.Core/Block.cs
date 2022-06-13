namespace Dct.Core
{
    public sealed class Block
    {
        private double[,] block = new double[8, 8];

        public double get(int x, int y)
        {
            return block[x, y];
        }

        public void set(int x, int y, double data)
        {
            this.block[x, y] = data;
        }
    }
}
