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

namespace PV2_zadanie
{
    public partial class Form1 : Form
    {
        private Image<Bgr, byte> img;
        private VideoCapture _capture1 = null;
        private VideoCapture _capture2 = null;
        private VideoCapture _capture3 = null;
        private bool _captureInProgress;
        private Mat _frame1;
        private Mat _frame2;
        private Mat _frame3;
        //private Mat _grayFrame;
        //private Mat _smallGrayFrame;
        //private Mat _smoothedGrayFrame;
        //private Mat _cannyFrame;

        private int count1;
        private int count2;
        private string path;

        public Form1()
        {
            InitializeComponent();
            CvInvoke.UseOpenCL = false;
            try
            {
                _capture1 = new VideoCapture(1); //0 = built in webcam, 1-2 = USB connected webcams
                _capture2 = new VideoCapture(0);
                _capture3 = new VideoCapture(2);
                _capture1.ImageGrabbed += ProcessFrame;
                _capture2.ImageGrabbed += ProcessFrame;
                _capture3.ImageGrabbed += ProcessFrame;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
            _frame1 = new Mat();
            _frame2 = new Mat();
            _frame3 = new Mat();
            //_grayFrame = new Mat();
            //_smallGrayFrame = new Mat();
            //_smoothedGrayFrame = new Mat();
            //_cannyFrame = new Mat();

            //create directory for saving images
            count1 = 0;
            count2 = 0;
            DateTime dt = DateTime.Now;
            path = "C:/Users/Miroslav Gajdzik/Desktop/PV2/zadanie/PV2_zadanie/PV2_zadanie/Pictures/";
            path += dt.ToString("dd.MM.yyyy_hh.mm.ss");
            DirectoryInfo di = Directory.CreateDirectory(path);
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            if ((_capture1 != null && _capture1.Ptr != IntPtr.Zero) || (_capture2 != null && _capture2.Ptr != IntPtr.Zero) || (_capture3 != null && _capture3.Ptr != IntPtr.Zero))
            {
                _capture1.Retrieve(_frame1, 0);
                _capture2.Retrieve(_frame2, 0);
                _capture3.Retrieve(_frame3, 0);

                //CvInvoke.CvtColor(_frame, _grayFrame, ColorConversion.Bgr2Gray);

                //CvInvoke.PyrDown(_grayFrame, _smallGrayFrame);

                //CvInvoke.PyrUp(_smallGrayFrame, _smoothedGrayFrame);

                //CvInvoke.Canny(_smoothedGrayFrame, _cannyFrame, 100, 60);

                builtWebCam_imageBox.Image = _frame1;
                USBCam1_imageBox.Image = _frame2;
                USBCam2_imageBox.Image = _frame3;
                //grayscaleImageBox.Image = _grayFrame;
                //smoothedGrayscaleImageBox.Image = _smoothedGrayFrame;
                //cannyImageBox.Image = _cannyFrame;
            }
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

        private void captureButton_Click(object sender, EventArgs e)
        {
            if (_capture1 != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Text = "Start Capture";
                    _capture1.Pause();
                }
                else
                {
                    //start the capture
                    captureButton.Text = "Stop";
                    _capture1.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
            if (_capture2 != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Text = "Start Capture";
                    _capture2.Pause();
                }
                else
                {
                    //start the capture
                    captureButton.Text = "Stop";
                    _capture2.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
            if (_capture3 != null)
            {
                if (_captureInProgress)
                {  //stop the capture
                    captureButton.Text = "Start Capture";
                    _capture3.Pause();
                }
                else
                {
                    //start the capture
                    captureButton.Text = "Stop";
                    _capture3.Start();
                }

                _captureInProgress = !_captureInProgress;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (USBCam1_imageBox.Image != null)
            {
                Bitmap frame1 = USBCam1_imageBox.Image.Bitmap;
                frame1_imageBox.Image = new Image<Bgr, byte>(frame1);

                //saving images
                try
                {
                    frame1.Save(path + "/usbCam1_" + count1++.ToString() + ".jpeg");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Saving image failed: {0}", ex.ToString());
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (USBCam2_imageBox.Image != null)
            {
                Bitmap frame2 = USBCam2_imageBox.Image.Bitmap;
                frame2_imageBox.Image = new Image<Bgr, byte>(frame2);

                //saving images
                try
                {
                    frame2.Save(path + "/usbCam2_" + count2++.ToString() + ".jpeg");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Saving image failed: {0}", ex.ToString());
                }
            }
        }

        private void ButtonCalibrate_Click(object sender, EventArgs e)
        {
            string path = "C:/Users/Miroslav Gajdzik/Desktop/PV2/zadanie/PV2_zadanie/PV2_zadanie/Pictures/Calibration/Z aplikácie/";

            string[] fileNames = Directory.GetFiles(path);
            List<string> fileList = fileNames.ToList<string>();

            Size chessSize = new Size(9, 6);

            Kalibrator kalibrator = new Kalibrator(fileList, chessSize);
            Mat camMatrix = kalibrator.cameraMatrix;
            Mat distCoefs = kalibrator.distortionCoeffs;
            
            //Image<Bgr, byte> bgrvstup = new Image<Bgr, byte>((Bitmap)frame1_imageBox.Image);
            //Image<Gray, byte> vstup = new Image<Gray, byte>(frame1_imageBox.Image);
        }

        //private void ReleaseData()
        //{
        //    if (_capture != null)
        //        _capture.Dispose();
        //}


    }
}
