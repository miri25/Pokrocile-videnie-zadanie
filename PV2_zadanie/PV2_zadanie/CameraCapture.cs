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
        private Calibration calibration = new Calibration();

        private int _maxSelect = 2;

        // flags
        private bool _isCalibrated;
        private bool _isSelected;

        // number of saved image
        private int imgNum = 0;

        // path to saving directory
        private String pathSave;
        private String pathLoad;

        public CameraCapture()
        {
            InitializeComponent();

            DateTime date = DateTime.Now;
            pathSave = Application.StartupPath + "\\Pictures\\Stereo";
            pathLoad = pathSave;
            //path += date.ToString("dd.MM.yyyy_hh.mm.ss");
            Directory.CreateDirectory(pathSave);

            CvInvoke.UseOpenCL = false;
            try
            {
                _camera = getImageCameras();

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
                camera.Add(new CameraInfo(i, CameraType.Stream));
            return camera;
        }

        private List<CameraInfo> getImageCameras()
        {
            string[] paths = null;
            using (OpenFileDialog fdialog = new OpenFileDialog())
            {
                fdialog.InitialDirectory = pathLoad;
                fdialog.Multiselect = true;
                fdialog.CheckFileExists = true;
                fdialog.CheckPathExists = true;
                fdialog.Filter = "(*.jpeg)|*.jpeg";

                fdialog.Title = "Camera image";
                if (fdialog.ShowDialog() == DialogResult.OK)
                {
                    paths = fdialog.FileNames;
                    pathLoad = fdialog.FileName.TrimStart('\\');
                }
            }
            
            List<CameraInfo> camera = new List<CameraInfo>();

            if (paths == null)
                return camera;

            for (int i = 0; i < paths.Length; i++)
                camera.Add(new CameraInfo(i, CameraType.Image, paths[i]));
            return camera;
        }

        private List<CameraInfo> getVideoCameras()
        {
            string[] paths = null;
            using (OpenFileDialog fdialog = new OpenFileDialog())
            {
                fdialog.InitialDirectory = pathLoad;
                fdialog.Multiselect = true;
                fdialog.CheckFileExists = true;
                fdialog.CheckPathExists = true;
                fdialog.Filter = "(*.mp4)|*.mp4";

                fdialog.Title = "Camera image";
                if (fdialog.ShowDialog() == DialogResult.OK)
                {
                    paths = fdialog.FileNames;
                    pathLoad = fdialog.FileName.TrimStart('\\');
                }
            }

            List<CameraInfo> camera = new List<CameraInfo>();
            for (int i = 0; i < paths.Length; i++)
                camera.Add(new CameraInfo(i, CameraType.Video, paths[i]));
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

        private void buttonSave_Click(object sender, EventArgs e)
        {
            if (_isSelected)
            {
                _selectedCam.ForEach(cam => cam.Save(pathSave, imgNum));
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
                {
                    c.Image = calibration.computeDisparity( _selectedCam[0].corrFrame,
                                                            _selectedCam[1].corrFrame).ToImage<Gray, byte>();
                    continue;
                }

                CameraInfo camera = _camera.First(cam => cam.id == (int)c.Tag);
                if (_isCalibrated)
                {
                    calibration.correctImage(camera.frame, camera.corrFrame, camera.role);
                    c.Image = camera.corrFrame;
                    continue;
                }

                c.Image = camera.frame;
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
                        fdialog.InitialDirectory = pathLoad;
                        fdialog.Multiselect = true;
                        fdialog.CheckFileExists = true;
                        fdialog.CheckPathExists = true;
                        fdialog.Filter = "(*.jpeg)|*.jpeg";

                        fdialog.Title = "Left camera images";
                        if (fdialog.ShowDialog() == DialogResult.OK)
                        {
                            paths = fdialog.FileNames;
                            pathLoad = fdialog.FileName.TrimStart('\\');
                        }
                    }

                    if (paths == null)
                    {
                        MessageBox.Show("Samples not set!");
                        return;
                    }

                    _isCalibrated = calibration.calibrate(25, new Size(8, 6), paths);

                    break;

                case 2:
                    _selectedCam[0].role = CameraRole.Stereo_Left;
                    _selectedCam[1].role = CameraRole.Stereo_Right;

                    string[] pathsLeft = null;
                    string[] pathsRight = null;
                    using (OpenFileDialog fdialog = new OpenFileDialog())
                    {
                        fdialog.InitialDirectory = pathLoad;
                        fdialog.Multiselect = true;
                        fdialog.CheckFileExists = true;
                        fdialog.CheckPathExists = true;
                        fdialog.Filter = "(*.jpeg)|*.jpeg";

                        fdialog.Title = "Left camera images";
                        if (fdialog.ShowDialog() == DialogResult.OK)
                        {
                            pathsLeft = fdialog.FileNames;
                            pathLoad = fdialog.FileName.TrimStart('\\');
                        }

                        fdialog.InitialDirectory = pathLoad;

                        fdialog.Title = "Right camera images";
                        if (fdialog.ShowDialog() == DialogResult.OK)
                        {
                            pathsRight = fdialog.FileNames;
                            pathLoad = fdialog.FileName.TrimStart('\\');
                        }
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

                    _isCalibrated = calibration.calibrate(25, new Size(9, 6), pathsLeft, pathsRight);

                    //addCamView(20);
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

        private void buttonSaveCalib_Click(object sender, EventArgs e)
        {
            string path = null;
            using (FolderBrowserDialog fdialog = new FolderBrowserDialog())
            {
                fdialog.RootFolder = Environment.SpecialFolder.CommonDesktopDirectory;

                if (fdialog.ShowDialog() == DialogResult.OK)
                    path = fdialog.SelectedPath;
            }
            calibration.saveData(path);
        }
    }

    class CameraInfo
    {
        // identifier
        public int id;

        public CameraRole role;

        public CameraType cameraType;

        // graber for capturing camera frame
        private VideoCapture _graber;

        // corrected frame
        public Image<Bgr,byte> corrFrame;

        // frame from camera
        public Image<Bgr, byte> frame;

        public CameraInfo(int id, CameraType cameraType, string source = null)
        {
            this.id = id;

            switch (cameraType)
            {
                case CameraType.Image:
                    frame = new Image<Bgr, byte>(source);
                    break;
                case CameraType.Video:
                    _graber = new VideoCapture(source);
                    _graber.ImageGrabbed += processFrame;
                    frame = new Image<Bgr, byte>(_graber.Width, _graber.Height);
                    break;

                case CameraType.Stream:
                    _graber = new VideoCapture(id);
                    _graber.ImageGrabbed += processFrame;
                    frame = new Image<Bgr, byte>(_graber.Width, _graber.Height);
                    break;
            }

            corrFrame = new Image<Bgr, byte>(frame.Width, frame.Height);
            this.cameraType = cameraType;           
        }

        public void Dispose()
        {
            if(cameraType != CameraType.Image)
                _graber.Dispose();
        }

        private void processFrame(object sender, EventArgs args)
        {
            if (cameraType != CameraType.Image)
                if (_graber.IsOpened)
                    _graber.Retrieve(frame, 0);
        }

        public void Start()
        {
            if (cameraType != CameraType.Image)
                if (_graber.IsOpened)
                    _graber.Start();
        }

        public void Stop()
        {
            if (cameraType != CameraType.Image)
                if (_graber.IsOpened)
                    _graber.Stop();
        }

        public void Pause()
        {
            if (cameraType != CameraType.Image)
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
