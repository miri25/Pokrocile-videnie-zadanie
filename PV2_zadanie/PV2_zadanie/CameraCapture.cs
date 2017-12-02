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
    public partial class CameraCapture : Form
    {
        // cameras
        List<CameraInfo> camera = new List<CameraInfo>();

        // select 2 for stereo
        private List<CameraInfo> _selectedCam = new List<CameraInfo>();

        private int _maxSelect = 2;

        // is capturing flag
        private bool _isCapturing;

        // is selected flag
        private bool _isSelected;

        // number of saved image
        private int imgNum = 0;

        // path to saving directory
        private string path;

        public CameraCapture()
        {
            InitializeComponent();

            DateTime date = DateTime.Now;
            path = "Pictures/";
            path += date.ToString("dd.MM.yyyy_hh.mm.ss");
            Directory.CreateDirectory(path);

            CvInvoke.UseOpenCL = false;
            try
            {
                DsDevice[] _SysteCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
                if (_SysteCameras.Length == 0)
                    return;

                for (int i = 0; i < _SysteCameras.Length; i++)
                    camera.Add(new CameraInfo(i, _SysteCameras[i].Name));

                setCamView(camera);
                frameTimer.Enabled = true;
                buttonCapture.Enabled = true;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        private void setCamView(List<CameraInfo> cam)
        {
            flowLayoutPanel.Controls.Clear();

            Size size = flowLayoutPanel.Size;
            int rows = (int)Math.Sqrt(cam.Count);
            int cols = (int)Math.Ceiling((double)cam.Count / rows);
            size.Height /= rows;
            size.Width /= cols;

            for (int i = 0; i < cam.Count; i++)
            {
                ImageBox view = new ImageBox();
                view.Tag = cam[i].id;
                view.MouseClick += camView_Click;
                view.Size = size;
                view.Margin = new Padding(0);
                view.Padding = new Padding(0);
                view.FunctionalMode = ImageBox.FunctionalModeOption.RightClickMenu;
                view.SizeMode = PictureBoxSizeMode.Zoom;

                flowLayoutPanel.Controls.Add(view);
            }
            return;
        }

        private void addCamView(CameraInfo cam)
        {
            Size size = flowLayoutPanel.Size;
            int count = flowLayoutPanel.Controls.Count + 1;

            int rows = (int)Math.Sqrt(count);
            int cols = (int)Math.Ceiling((double)count / rows);
            size.Height /= rows;
            size.Width /= cols;

            ImageBox view = new ImageBox();
            view.Tag = cam.id;
            view.MouseClick += camView_Click;
            view.Margin = new Padding(0);
            view.Padding = new Padding(0);
            view.FunctionalMode = ImageBox.FunctionalModeOption.RightClickMenu;
            view.SizeMode = PictureBoxSizeMode.Zoom;

            flowLayoutPanel.Controls.Add(view);

            foreach (Control c in flowLayoutPanel.Controls)
                c.Size = size;

            return;
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            _isCapturing = !_isCapturing;

            if (_isCapturing)
            {
                buttonCapture.Text = "Stop";
                camera.ForEach(cam => cam.Start());
                frameTimer.Start();
                buttonSelect.Enabled = true;
            }
            else
            {
                buttonCapture.Text = "Start";
                camera.ForEach(cam => cam.Pause());
                frameTimer.Stop();
                buttonSelect.Enabled = false;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (_isCapturing && _isSelected)
            {
                _selectedCam.ForEach(cam => cam.Save(path, imgNum));
                imgNum++;
            }
        }

        private void buttonSelect_Click(object sender, EventArgs e)
        {
            _isSelected = !_isSelected;

            if (_isSelected)
            {
                buttonSelect.Text = "Reset";
                setCamView(_selectedCam);
                foreach (CameraInfo cam in camera)
                    if(!_selectedCam.Contains(cam))
                        cam.Stop();
                buttonSave.Enabled = true;
            }
            else
            {
                buttonSelect.Text = "Select";
                _selectedCam.Clear();
                camera.ForEach(cam => cam.Start());
                setCamView(camera);
                buttonSave.Enabled = false;
            }
        }

        private void camView_Click(object sender, EventArgs e)
        {
            if (!_isSelected)
            {
                ImageBox control = (sender as ImageBox);
                CameraInfo selectedCam = camera.First(cam => cam.id == (int)control.Tag);
                if (_selectedCam.Remove(selectedCam))
                {
                    control.BackColor = Color.Transparent;
                    return;
                }

                if (_selectedCam.Count == _maxSelect)
                    return;

                _selectedCam.Add(selectedCam);
                control.BackColor = Color.DeepSkyBlue;
                labelSelect.Text += "\nCamera_" + selectedCam.id; 
            }
        }

        private void frameTimer_Tick(object sender, EventArgs e)
        {
            foreach (ImageBox c in flowLayoutPanel.Controls)
                c.Image = camera.First(cam => cam.id == (int)c.Tag).frame;

        }

        private void CameraCapture_FormClosing(object sender, FormClosingEventArgs e)
        {
            frameTimer.Stop();
            camera.ForEach(cam => cam.Stop());
            camera.ForEach(cam => cam.Dispose());
            camera.Clear();
            _selectedCam.Clear();
        }
    }

    class CameraInfo
    {
        // identifier
        public int id;

        public string cameraType;

        // graber for capturing camera frame
        private VideoCapture _graber;

        // frame from camera
        public Mat frame;

        public CameraInfo(int id, string cameraType)
        {
            this.id = id;
            this.cameraType = cameraType.Replace(' ', '_');
            _graber = new VideoCapture(id);
            _graber.ImageGrabbed += processFrame;
            frame = new Mat();
        }

        public void Dispose()
        {
            _graber.Dispose();
        }

        private void processFrame(object sender, EventArgs args)
        {
            if (_graber.IsOpened)
                _graber.Retrieve(frame, 0);
        }

        public void Start()
        {
            if (_graber.IsOpened)
                _graber.ImageGrabbed += processFrame;
        }

        public void Stop()
        {
            if (_graber.IsOpened)
                _graber.Stop();
        }

        public void Pause()
        {
            if (_graber.IsOpened)
                _graber.Pause();
        }

        public bool Save(string path, int imgNum)
        {
            string cameraPath = path + "/Camera_" + id;
            if (!Directory.Exists(cameraPath))
                Directory.CreateDirectory(cameraPath);

            try
            {
                frame.Save(cameraPath + "/" + imgNum + ".jpeg");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Saving image failed: {0}", ex.ToString());
                return false;
            }
            return true;
        }
    }
}
