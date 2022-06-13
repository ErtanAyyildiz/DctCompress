namespace Dct.Core
{
    public interface ICompressor
    {
        double[,] YImage { get; set; }
        double[,] CrImage { get; set; }
        double[,] CbImage { get; set; }

        void Compress();
        void OpenSavedFile(string file);
    }
}
