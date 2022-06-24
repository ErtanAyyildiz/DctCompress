namespace Dct.Core
{
    public interface ICompressor
    {
        bool IsMultithreadingOperation { get; set; }
        double[,] YImage { get; set; }
        double[,] CrImage { get; set; }
        double[,] CbImage { get; set; }

        void Compress();
        void CompressAsync();
        void OpenSavedFile(string file);
    }
}
