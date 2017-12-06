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
        private List<CameraInfo> _camera = new List<CameraInfo>();
        private List<CameraInfo> _selectedCam = new List<CameraInfo>();

        // calibrators
        private Calibration stereoCalib = new Calibration();

        private int _maxSelect = 2;

        // flags
        private bool _isCalibrated;
        private bool _isSelected;

        // number of saved image
        private int imgNum = 0;

        // path to saving directory
        private string path;

        public CameraCapture()
        {
            InitializeComponent();

            DateTime date = DateTime.Now;
            path = Application.StartupPath + "\\Pictures\\Stereo";
            //path += date.ToString("dd.MM.yyyy_hh.mm.ss");
            //Directory.CreateDirectory(path);

            CvInvoke.UseOpenCL = false;
            try
            {
                _camera = getConnectedCameras();

                if (_camera.Count == 0)
                    return;
                
                setCamView(_camera);
                _camera.ForEach(cam => cam.Start());
                frameTimer.Enabled = true;
            }
            catch (NullReferenceException excpt)
            {
                MessageBox.Show(excpt.Message);
            }
        }

        private List<CameraInfo> getConnectedCameras()
        {
            DsDevice[] _SysteCameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            if (_SysteCameras.Length == 0)
                return new List<CameraInfo>();

            List<CameraInfo> camera = new List<CameraInfo>();
            for (int i = 0; i < _SysteCameras.Length; i++)
                camera.Add(new CameraInfo(i, _SysteCameras[i].Name));
            return camera;
        }

        private void setCamView(List<CameraInfo> camera)
        {
            flowLayoutPanel.Controls.Clear();

            Size size = flowLayoutPanel.Size;
            int rows = (int)Math.Sqrt(camera.Count);
            int cols = (int)Math.Ceiling((double)camera.Count / rows);
            size.Height /= rows;
            size.Width /= cols;

            for (int i = 0; i < camera.Count; i++)
            {
                ImageBox view = new ImageBox();
                view.Tag = camera[i].id;
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

        private void addCamView(int idTag)
        {
            Size size = flowLayoutPanel.Size;
            int count = flowLayoutPanel.Controls.Count + 1;

            int rows = (int)Math.Sqrt(count);
            int cols = (int)Math.Ceiling((double)count / rows);
            size.Height /= rows;
            size.Width /= cols;

            ImageBox view = new ImageBox();
            view.Tag = idTag;
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

        private void addRowView(int idTag)
        {
            Size size = flowLayoutPanel.Size;
            int count = flowLayoutPanel.Controls.Count;

            int rows = (int)Math.Sqrt(count);
            int cols = (int)Math.Ceiling((double)count / rows);
            size.Height /= rows+1;
            size.Width /= cols;

            foreach (Control c in flowLayoutPanel.Controls)
                c.Size = size;

            ImageBox view = new ImageBox();
            view.Tag = idTag;
            view.Margin = new Padding(0);
            view.Padding = new Padding(0);
            view.FunctionalMode = ImageBox.FunctionalModeOption.RightClickMenu;
            view.SizeMode = PictureBoxSizeMode.Zoom;
            view.Size = new Size(flowLayoutPanel.Width, size.Height);
            flowLayoutPanel.Controls.Add(view);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (_isSelected)
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
                buttonSave.Enabled = true;
                buttonCalibration.Enabled = true;

                setCamView(_selectedCam);
                foreach (CameraInfo cam in _camera)
                    if (!_selectedCam.Contains(cam))
                        cam.Stop();
            }
            else
            {
                buttonSelect.Text = "Select";
                buttonSave.Enabled = false;
                buttonCalibration.Enabled = false;
                _isCalibrated = false;

                _selectedCam.Clear();
                _camera.ForEach(cam => cam.Start());
                setCamView(_camera);
            }
        }

        private void camView_Click(object sender, EventArgs e)
        {
            if (!_isSelected)
            {
                ImageBox control = (sender as ImageBox);
                CameraInfo selectedCam = _camera.First(cam => cam.id == (int)control.Tag);
                if (_selectedCam.Remove(selectedCam))
                {
                    control.BackColor = Color.Transparent;
                    return;
                }

                if (_selectedCam.Count == _maxSelect)
                    return;

                _selectedCam.Add(selectedCam);
                control.BackColor = Color.DeepSkyBlue;
                labelSelect.Text = "";
                _selectedCam.ForEach(selCam => labelSelect.Text += "\nCamera_" + selCam.id);
            }
        }

        private void frameTimer_Tick(object sender, EventArgs e)
        {
            foreach (ImageBox c in flowLayoutPanel.Controls)
            {
                if ((int)c.Tag == 20)
                    c.Image = stereoCalib.computeDisparity(_selectedCam[0].frame, _selectedCam[1].frame).ToImage<Gray, byte>();
                else
                {
                    CameraInfo camera = _camera.First(cam => cam.id == (int)c.Tag);
                    if (_isCalibrated)
                        if ((int)c.Tag == 20)
                            c.Image = stereoCalib.computeDisparity(_selectedCam[0].frame, _selectedCam[1].frame);
                        else
                            c.Image = stereoCalib.correctImage(camera.frame, camera.role);
                    else
                        c.Image = camera.frame;
                }
            }

        }

        private void buttonCalibration_Click(object sender, EventArgs e)
        {
            switch (_selectedCam.Count)
            {
                case 1:
                    _selectedCam[0].role = CameraRole.Single;
                    string[] paths = null;
                    using (OpenFileDialog fdialog = new OpenFileDialog())
                    {
                        fdialog.InitialDirectory = path;
                        fdialog.Multiselect = true;
                        fdialog.CheckFileExists = true;
                        fdialog.CheckPathExists = true;
                        fdialog.Filter = "(*.jpeg)|*.jpeg";

                        fdialog.Title = "Left camera images";
                        if (fdialog.ShowDialog() == DialogResult.OK)
                            paths = fdialog.FileNames;
                    }

                    if (paths == null)
                    {
                        MessageBox.Show("Samples not set!");
                        return;
                    }

                    _isCalibrated = stereoCalib.calibrate(25, new Size(9, 6), paths);

                    break;

                case 2:
                    _selectedCam[0].role = CameraRole.Stereo_Left;
                    _selectedCam[1].role = CameraRole.Stereo_Right;

                    string[] pathsLeft = null;
                    string[] pathsRight = null;
                    using (OpenFileDialog fdialog = new OpenFileDialog())
                    {
                        fdialog.InitialDirectory = path;
                        fdialog.Multiselect = true;
                        fdialog.CheckFileExists = true;
                        fdialog.CheckPathExists = true;
                        fdialog.Filter = "(*.jpeg)|*.jpeg";

                        fdialog.Title = "Left camera images";
                        if (fdialog.ShowDialog() == DialogResult.OK)
                            pathsLeft = fdialog.FileNames;
                        
                        fdialog.Title = "Right camera images";
                        if (fdialog.ShowDialog() == DialogResult.OK)
                            pathsRight = fdialog.FileNames;
                    }

                    if (pathsLeft == null || pathsRight == null)
                    {
                        MessageBox.Show("Samples not set!");
                        return;
                    }

                    if (pathsLeft.Length != pathsRight.Length)
                    {
                        MessageBox.Show("Samples count is not equal!");
                        return;
                    }

                    _isCalibrated = stereoCalib.calibrate(25, new Size(9, 6), pathsLeft, pathsRight);

                    addCamView(20);
                    break;
            }
        }

        private void CameraCapture_FormClosing(object sender, FormClosingEventArgs e)
        {
            frameTimer.Stop();
            _camera.ForEach(cam => cam.Stop());
            _camera.ForEach(cam => cam.Dispose());
            _camera.Clear();
            _selectedCam.Clear();
        }
    }

    class CameraInfo
    {
        // identifier
        public int id;

        public CameraRole role;

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
                _graber.Start();
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
            string cameraPath = path + "\\Camera_" + id;
            if (!Directory.Exists(cameraPath))
                Directory.CreateDirectory(cameraPath);

            try
            {
                frame.Save(cameraPath + "\\" + imgNum + ".jpeg");
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
