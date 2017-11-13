using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.CV.UI;

namespace PV2_zadanie
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> img;

        public Form1()
        {
            InitializeComponent();
        }

        // test open image
        private void imageBox1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    img = new Image<Bgr, byte>(ofd.FileName);
                    imageBox1.Image = img;
                }
            }
        }
    }
}
