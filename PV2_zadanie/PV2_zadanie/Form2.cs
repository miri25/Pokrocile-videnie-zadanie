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
using Emgu.CV.CvEnum;
using System.IO;
using DirectShowLib;


namespace PV2_zadanie
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        private void buttonImgCapture_Click(object sender, EventArgs e)
        {
            CameraCapture capture = new CameraCapture();
            capture.ShowDialog();
        }

        private void buttonCalibration_Click(object sender, EventArgs e)
        {
            // LoadImgs or select directory
            // Calibrate
        }
    }
}
