namespace Dct.Core
{
    public sealed class Block
    {
        private readonly double[,] block = new double[8, 8];
        public double this[int x, int y]
        {
            get => block[x, y];
            set => block[x, y] = value;
        }
    }
}
