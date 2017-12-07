﻿using System;
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
        private Matrix<double>[] cameraMatrix;
        private Matrix<double>[] distortionCoeffs;
        private Matrix<double>[] rotationMatrix;
        private Matrix<double>[] translationMatrix;
        private Matrix<double> disparityMatrix;
        private Rectangle[] roiCam;
        private Size imgSize;

        private bool _isCalibrated = false;
        public bool isCalibrated { get { return _isCalibrated; } }

        private bool _isStereo;

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
            roiCam = new Rectangle[1];

            cameraMatrix[0] = new Matrix<double>(3, 3);
            distortionCoeffs[0] = new Matrix<double>(8, 1);
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
            if (!_isCalibrated)
                return null;

            Mat outImg = image.Clone();

            if (!_isStereo)
            {
                CvInvoke.Undistort(image, outImg, cameraMatrix[0], distortionCoeffs[0]);
                /*
                roiCam[0] = new Rectangle();
                CvInvoke.InitUndistortRectifyMap(
                    cameraMatrix[0],
                    distortionCoeffs[0],
                    new Mat(),
                    CvInvoke.GetOptimalNewCameraMatrix(
                        cameraMatrix[0],
                        distortionCoeffs[0],
                        imgSize, 1, imgSize,
                        ref roiCam[0]),
                    imgSize,
                    DepthType.Cv32F,
                    map1, map2);
                CvInvoke.Remap(image, outImg, map1, map2, Inter.Cubic);
                */
                
                return outImg;
            }

            int ix = 0;
            if (role == CameraRole.Stereo_Right)
                ix = 1;

            //CvInvoke.Undistort(image, outImg, cameraMatrix[ix], distortionCoeffs[ix]);
            Mat map1 = new Mat();
            Mat map2 = new Mat();

            //CvInvoke.WarpPerspective(outImg, outImg, translationMatrix[ix],imgSize);
            
            CvInvoke.InitUndistortRectifyMap(
                cameraMatrix[ix],
                distortionCoeffs[ix],
                rotationMatrix[ix],
                translationMatrix[ix],
                imgSize,
                DepthType.Cv32F,
                map1, map2);
            CvInvoke.Remap(image, outImg, map1, map2, Inter.Cubic);
            return outImg;
        }

        public Mat computeDisparity(Mat leftImg, Mat rightImg)
        {
            if (!_isCalibrated || !_isStereo)
                return new Mat();
            
            if (leftImg.Depth != rightImg.Depth)
                return new Mat();
            
            Mat disparity = new Mat();
            using (StereoSGBM stereoSolver = new StereoSGBM(5, 64, 3))
            {
                stereoSolver.Compute(leftImg, rightImg, disparity);
                //CvInvoke.ReprojectImageTo3D(disparity, img, disparityMatrix);
                //MCvPoint3D32f[] points = PointCollection.ReprojectImageTo3D(disparity, disparityMatrix);
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

            VectorOfPoint3D32F chessboard = getChessboardCorners(squareEdge, patternSize);
            VectorOfVectorOfPoint3D32F objectPoints = new VectorOfVectorOfPoint3D32F();
            for (int i = corners.Size; i > 0; i--)
                objectPoints.Push(chessboard);

            Image<Gray, Byte> image = new Image<Gray, Byte>(images[0]);
            imgSize = image.Size;
            Mat rotationMat = new Mat();
            Mat translationMat = new Mat();

            CvInvoke.CalibrateCamera(
                objectPoints,
                corners,
                image.Size,
                cameraMatrix[0],
                distortionCoeffs[0],
                rotationMat,
                translationMat,
                CalibType.Default,
                new MCvTermCriteria(30, 0.1));
            
            return _isCalibrated = true;
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
                image.Size,
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
                image.Size,
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

            imgSize.Width = roiCam[0].Width > roiCam[1].Width ? roiCam[0].Width : roiCam[1].Width;
            imgSize.Height = roiCam[0].Height > roiCam[1].Height ? roiCam[0].Height : roiCam[1].Height;

            return _isCalibrated = true;
        }

        public void saveData(string path)
        {
            if (!_isCalibrated)
                return;

            StreamWriter file = new StreamWriter(path, false);
            for (int cam = 0; cam < cameraMatrix.Length; cam++)
            {
                file.WriteLine("Camera Matrix "+ cam +":");
                for (int row = 0; row < cameraMatrix[cam].Rows; row++)
                {
                    for (int col = 0; col < cameraMatrix[cam].Cols; col++)
                    {
                        file.Write("{0,10:F3}", cameraMatrix[cam][row, col]);
                    }
                    file.WriteLine();
                }
                file.WriteLine();

                file.WriteLine("Distortion Matrix "+ cam +":");
                for (int row = 0; row < distortionCoeffs[cam].Rows; row++)
                {
                    for (int col = 0; col < distortionCoeffs[cam].Cols; col++)
                    {
                        file.Write("{0,10:F3}", distortionCoeffs[cam][row, col]);
                    }
                    file.WriteLine();
                }
            }
        }
    }
}
