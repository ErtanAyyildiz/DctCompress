using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace Dct.UI
{
    public partial class FormUncompress : Form
    {
        public FormUncompress(Bitmap bitmap)
        {
            InitializeComponent();
            pictureBox1.Image = bitmap;
        }

        void CmsiSaveImage_Click(object sender, System.EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog() { FileName = "compressed.jpeg" };
            DialogResult dialog = save.ShowDialog();
            if (dialog == DialogResult.OK)
            {
                pictureBox1.Image.Save(save.FileName, ImageFormat.Jpeg);
                Process.Start(save.FileName);
            }
        }
    }
}
