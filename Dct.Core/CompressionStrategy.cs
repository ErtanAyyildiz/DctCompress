namespace Dct.Core
{
    public sealed class CompressionStrategy
    {
        /*
         * Strategy pattern: Bağımlılıkları azaltmak için (Dependency Inversion Principle) kullanılan bir
         * OO tasarım desenidir.
         */
        private readonly ICompressor compressor;
        public ICompressor Compressor => compressor;

        public CompressionStrategy(ICompressor compressor)
        {
            this.compressor = compressor;
        }

        public double[,] YImage
        {
            get => compressor.YImage;
            set => compressor.YImage = value;
        }

        public double[,] CrImage
        {
            get => compressor.CrImage;
            set => compressor.CrImage = value;
        }

        public double[,] CbImage
        {
            get => compressor.CbImage;
            set => compressor.CbImage = value;
        }

        public void Compress()
        {
            Compressor.IsMultithreadingOperation = false;
            Compressor.Compress();
        }

        public void CompressAsync()
        {
            Compressor.IsMultithreadingOperation = true;
            Compressor.CompressAsync();
        }

        public void OpenSavedFile(string file)
        {
            Compressor.OpenSavedFile(file);
        }

        public void SetMultiThreadOperations(bool flag)
        {
            Compressor.IsMultithreadingOperation = flag;
        }
    }
}
