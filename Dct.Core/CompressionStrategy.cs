namespace Dct.Core
{
    public sealed class CompressionStrategy
    {
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
            Compressor.Compress();
        }

        public void OpenSavedFile(string file)
        {
            Compressor.OpenSavedFile(file);
        }
    }
}
