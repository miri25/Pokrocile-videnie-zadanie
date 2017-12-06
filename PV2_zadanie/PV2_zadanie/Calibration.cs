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
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV.CvEnum;
using System.IO;

namespace PV2_zadanie
{
    enum CameraRole { Single, Stereo_Left, Stereo_Right };

    class Calibration
    {
        Matrix<double>[] cameraMatrix;
        Matrix<double>[] distortionCoeffs;
        Matrix<double>[] rotationMatrix;
        Matrix<double>[] translationMatrix;
        Matrix<double> disparityMatrix;
        Rectangle[] roiCam;
        Size imgSize;

        bool _isStereo;

        public Calibration()
        {
            
        }

        public void initSingle()
        {
            _isStereo = false;
            cameraMatrix = new Matrix<double>[1];
            distortionCoeffs = new Matrix<double>[1];
            rotationMatrix = new Matrix<double>[0];
            translationMatrix = new Matrix<double>[0];

            cameraMatrix[1] = new Matrix<double>(3, 3);
            distortionCoeffs[1] = new Matrix<double>(8, 1);
        }

        public void initStereo()
        {
            _isStereo = true;
            cameraMatrix = new Matrix<double>[2];
            distortionCoeffs = new Matrix<double>[2];
            rotationMatrix = new Matrix<double>[2];
            translationMatrix = new Matrix<double>[2];
            roiCam = new Rectangle[2];

            disparityMatrix = new Matrix<double>(4, 4);
            for (int i = 0; i < 2; i++)
            {
                cameraMatrix[i] = new Matrix<double>(3, 3);
                distortionCoeffs[i] = new Matrix<double>(1, 5);
                rotationMatrix[i] = new Matrix<double>(3, 3);
                translationMatrix[i] = new Matrix<double>(3, 4);
                roiCam[i] = Rectangle.Empty;
            }
        }

        private VectorOfPoint3D32F getChessboardCorners(float square, Size patternSize)
        {
            MCvPoint3D32f[] corners = new MCvPoint3D32f[patternSize.Height * patternSize.Width];

            for (int i = 0; i < patternSize.Height; i++)
                for (int j = 0; j < patternSize.Width; j++)
                    corners[i * patternSize.Width + j] = new MCvPoint3D32f(i * square, j * square, 0);
            return new VectorOfPoint3D32F(corners);
        }

        private List<VectorOfVectorOfPointF> findCorners(float squareEdge, Size patternSize, string[] imagesLeft, string[] imagesRight)
        {
            VectorOfVectorOfPointF allCornersLeft = new VectorOfVectorOfPointF();
            VectorOfVectorOfPointF allCornersRight = new VectorOfVectorOfPointF();
            VectorOfPointF cornersLeft = new VectorOfPointF();
            VectorOfPointF cornersRight = new VectorOfPointF();

            Image<Gray, Byte> imageLeft;
            Image<Gray, Byte> imageRight;
            bool findLeft, findRight;
            for (int i = 0; i < imagesLeft.Length; i++)
            {
                imageLeft = new Image<Gray, Byte>(imagesLeft[i]);
                imageRight = new Image<Gray, Byte>(imagesRight[i]);

                findLeft = CvInvoke.FindChessboardCorners(
                    imageLeft,
                    patternSize,
                    cornersLeft);

                findRight = CvInvoke.FindChessboardCorners(
                    imageRight,
                    patternSize,
                    cornersRight);

                if (!findLeft || !findRight)
                    continue;

                CvInvoke.CornerSubPix(
                    imageLeft,
                    cornersLeft,
                    new Size(11, 11),
                    new Size(-1, -1),
                    new MCvTermCriteria(30, 0.1));

                CvInvoke.CornerSubPix(
                    imageRight,
                    cornersRight,
                    new Size(11, 11),
                    new Size(-1, -1),
                    new MCvTermCriteria(30, 0.1));

                allCornersLeft.Push(cornersLeft);
                allCornersRight.Push(cornersRight);

                imageLeft.Dispose();
                imageRight.Dispose();
                GC.Collect();
            }

            return new List<VectorOfVectorOfPointF>() { allCornersLeft, allCornersRight};
        }

        private VectorOfVectorOfPointF findCorners(float squareEdge, Size patternSize, string[] imagePaths)
        {
            VectorOfVectorOfPointF allCorners = new VectorOfVectorOfPointF();
            VectorOfPointF corners = new VectorOfPointF();

            Image<Gray, Byte> image;
            bool find;
            for (int i = 0; i < imagePaths.Length; i++)
            {
                image = new Image<Gray, Byte>(imagePaths[i]);

                find = CvInvoke.FindChessboardCorners(
                    image,
                    patternSize,
                    corners);

                if (!find)
                    continue;

                CvInvoke.CornerSubPix(
                    image,
                    corners,
                    new Size(11, 11),
                    new Size(-1, -1),
                    new MCvTermCriteria(30, 0.1));

                allCorners.Push(corners);
                image.Dispose();
                GC.Collect();
            }

            return allCorners;
        }

        public Mat correctImage(Mat image, CameraRole role)
        {
            Mat outImg = image.Clone();
            int ix = 0;

            if (role == CameraRole.Stereo_Right && _isStereo)
                ix = 1;

            CvInvoke.Undistort(image, outImg, cameraMatrix[ix], distortionCoeffs[ix]);
            //outImg.Draw(roiCam[ix], new Bgr(Color.LimeGreen), 1);
            return outImg;
        }

        public Mat computeDisparity(Mat leftImg, Mat rightImg)
        {
            Mat disparity = new Mat();
            
            using (StereoSGBM stereoSolver = new StereoSGBM(5, 48, 0))
            {
                stereoSolver.Compute(leftImg, rightImg, disparity);
                //CvInvoke.ReprojectImageTo3D(disparity, img, disparityMatrix);
            }

            return disparity;
        }

        private void showDetectedCorners(int figure, string path, Point[] corners)
        {
            Random R = new Random();
            Image < Bgr, Byte > image = new Image<Bgr, Byte>(path);
            image.Draw(new CircleF(corners[0], 3), new Bgr(Color.Yellow), 1);
            for (int i = 1; i < corners.Length; i++)
            {
                image.Draw(new LineSegment2DF(corners[i - 1], corners[i]),
                    new Bgr(R.Next(0, 255), R.Next(0, 255), R.Next(0, 255)), 2);
                image.Draw(new CircleF(corners[i], 3), new Bgr(Color.Yellow), 1);
            }
            CvInvoke.Imshow("Image_"+ figure, image);
            System.Threading.Thread.Sleep(100);
        }

        public bool calibrate(float squareEdge, Size patternSize, string[] images)
        {
            initSingle();
            VectorOfVectorOfPointF corners = findCorners(squareEdge, patternSize, images);

            if (corners.Size == 0)
            {
                Console.WriteLine("Cannot find chessboard!");
                return false;
            }

            VectorOfVectorOfPoint3D32F chessboard = new VectorOfVectorOfPoint3D32F(getChessboardCorners(squareEdge, patternSize));

            Image<Gray, Byte> image = new Image<Gray, Byte>(images[0]);
            Size imgSize = image.Size;
            Mat rotationMat = new Mat();
            Mat translationMat = new Mat();

            CvInvoke.CalibrateCamera(
                chessboard,
                corners,
                image.Size,
                cameraMatrix[1],
                distortionCoeffs[1],
                rotationMat,
                translationMat,
                CalibType.Default,
                new MCvTermCriteria(30, 0.1));

            return true;
        }

        public bool calibrate(float squareEdge, Size patternSize, string[] imagesLeft, string[] imagesRight)
        {
            initStereo();
            List<VectorOfVectorOfPointF> listCorners = findCorners(squareEdge, patternSize, imagesLeft, imagesRight);

            if (listCorners.Last().Size == 0)
            {
                Console.WriteLine("Cannot find chessboard!");
                return false;
            }

            VectorOfPoint3D32F chessboard = getChessboardCorners(squareEdge, patternSize);
            VectorOfVectorOfPoint3D32F objectPoints = new VectorOfVectorOfPoint3D32F();
            for (int i = listCorners.Last().Size; i > 0; i--)
                objectPoints.Push(chessboard);

            Image<Gray, Byte> image = new Image<Gray, Byte>(imagesLeft[0]);
            Size frameSz = image.Size;
            imgSize = Size.Empty;
            roiCam[0] = Rectangle.Empty;
            roiCam[1] = Rectangle.Empty;
            
            // set mats
            Mat rotationMat = new Mat();
            Mat translationMat = new Mat();
            Mat essentialMat = new Mat();
            Mat fundamentalMat = new Mat();
            
            CvInvoke.StereoCalibrate(
                objectPoints,
                listCorners[0],
                listCorners[1],
                cameraMatrix[0],
                distortionCoeffs[0],
                cameraMatrix[1],
                distortionCoeffs[1],
                frameSz,
                rotationMat,
                translationMat,
                essentialMat,
                fundamentalMat,
                CalibType.Default,
                new MCvTermCriteria(30, 0.1e5));

            CvInvoke.StereoRectify(
                cameraMatrix[0],
                distortionCoeffs[0],
                cameraMatrix[1],
                distortionCoeffs[1],
                frameSz,
                rotationMat,
                translationMat,
                rotationMatrix[0],
                rotationMatrix[1],
                translationMatrix[0],
                translationMatrix[1],
                disparityMatrix,
                StereoRectifyType.Default,
                1,
                imgSize,
                ref roiCam[0],
                ref roiCam[1]);

            return true;
        }
    }
}
