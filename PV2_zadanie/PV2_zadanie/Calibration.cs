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

    enum CameraType { Stream, Video, Image };

    class CameraParam
    {
        public Matrix<double> cameraMatrix;
        public Matrix<double> distortionCoeffs;
        public Mat rotationMatrix;
        public Mat translationMatrix;

        public CameraParam()
        {
            cameraMatrix = new Matrix<double>(3,3);
            distortionCoeffs = new Matrix<double>(1,5);
            rotationMatrix = new Mat();
            translationMatrix = new Mat();
        }
        
    }

    class Calibration
    {
        List<CameraParam> cameraParam = new List<CameraParam>();
        private Mat disparityMatrix = new Mat();
        private Matrix<int>[] rectMask = new Matrix<int>[2];
        private Size imgSize;

        Matrix<double> R = new Matrix<double>(3,3);
        Matrix<double> T = new Matrix<double>(3, 1);
        
        private bool _isCalibrated = false;
        public bool isCalibrated { get { return _isCalibrated; } }

        private bool _isStereo;

        public Calibration()
        {
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

        public void correctImage(Image<Bgr,byte> src, Image<Bgr, byte> dest, CameraRole role)
        {
            if (!_isCalibrated)
                return;

            if (!_isStereo)
            {
                CvInvoke.Undistort(src, dest, cameraParam[0].cameraMatrix, cameraParam[0].distortionCoeffs);
                return;
            }

            int ix = 0;
            if (role == CameraRole.Stereo_Right)
                ix = 1;
            /*
            Mat map1 = new Mat();
            Mat map2 = new Mat();
            
            CvInvoke.InitUndistortRectifyMap(
                cameraParam[ix].cameraMatrix,
                cameraParam[ix].distortionCoeffs,
                cameraParam[ix].rotationMatrix,
                cameraParam[ix].translationMatrix,
                src.Size,
                DepthType.Cv32F,
                map1, map2);
            CvInvoke.Remap(src, dest, map1, map2, Inter.Cubic);
            return;
            */
            
            Image<Bgr, byte> temp = new Image<Bgr, byte>(src.Width, src.Height);
            CvInvoke.Undistort(src, temp, cameraParam[ix].cameraMatrix, cameraParam[ix].distortionCoeffs);
            rectification(temp, dest, ix);
            temp.Dispose();
            return;
            
        }

        private void rectification(Image<Bgr, byte> src, Image<Bgr, byte> dest, int ix)
        {
            //if (src.Size != dest.Size)
            //    dest = new Image<Bgr, byte>(src.Width, src.Height);

            for (int y = 0; y < src.Rows; y++)
                for (int x = 0; x < src.Cols; x++)
                    dest[rectMask[ix].Data[y, 2 * x], rectMask[ix].Data[y, 2 * x + 1]] = src[y,x];

            /*
            for (int y = 0; y < src.Rows; y++)
                if (y % 20 == 0)
                    CvInvoke.Line(dest, new Point(0, y), new Point(dest.Cols, y), new MCvScalar(0, 255, 0));
            */
        }


        public Mat computeDisparity(Image<Bgr, byte> leftImg, Image<Bgr, byte> rightImg)
        {
            if (!_isCalibrated || !_isStereo)
                return new Mat();
            
            if (leftImg.Mat.Depth != rightImg.Mat.Depth)
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

            CameraParam param = new CameraParam();
            
            // set mats
            Mat rotationMat = new Mat();
            Mat translationMat = new Mat();

            Image<Gray, Byte> image = new Image<Gray, Byte>(images[0]);
            imgSize = image.Size;

            CvInvoke.CalibrateCamera(
                objectPoints,
                corners,
                image.Size,
                param.cameraMatrix.Mat,
                param.distortionCoeffs.Mat,
                rotationMat,
                translationMat,
                CalibType.Default,
                new MCvTermCriteria(30, 0.1));

            cameraParam.Clear();
            cameraParam.Add(param);
            return _isCalibrated = true;
        }

        public bool calibrate(float squareEdge, Size patternSize, string[] imagesLeft, string[] imagesRight)
        {
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

            CameraParam camLeft = new CameraParam();
            CameraParam camRight = new CameraParam();
            
            Matrix<double> E = new Matrix<double>(3, 3);
            Matrix<double> F = new Matrix<double>(3, 3);
            CvInvoke.StereoCalibrate(
                objectPoints,
                listCorners[0],
                listCorners[1],
                camLeft.cameraMatrix.Mat,
                camLeft.distortionCoeffs.Mat,
                camRight.cameraMatrix.Mat,
                camRight.distortionCoeffs.Mat,
                image.Size,
                R, T, E, F,
                CalibType.Default,
                new MCvTermCriteria(30, 0.1e5));
            
            /*
            Rectangle roi1 = Rectangle.Empty, roi2 = Rectangle.Empty;
            CvInvoke.StereoRectify(
                camLeft.cameraMatrix.Mat,
                camLeft.distortionCoeffs.Mat,
                camRight.cameraMatrix.Mat,
                camRight.distortionCoeffs.Mat,
                image.Size,
                R,T,
                camLeft.rotationMatrix,
                camRight.rotationMatrix,
                camLeft.translationMatrix,
                camRight.translationMatrix,
                disparityMatrix,
                StereoRectifyType.Default,
                -1, Size.Empty, 
                ref roi1,
                ref roi2);
            */

            cameraParam.Clear();
            cameraParam.Add(camLeft);
            cameraParam.Add(camRight);

            //Matrix<double> newE = getEssentialMatrix();
            //Matrix<double> newF = getFundamentalMatrix();
            rectMask[0] = getPointSet(image.Size, 0);
            rectMask[1] = getPointSet(image.Size, 1);
            
            _isStereo = true;
            return _isCalibrated = true;
        }
        
        public Matrix<int> getPointSet(Size imgSz, int ix)
        {
            Matrix<int> outMat = new Matrix<int>(imgSz.Height, imgSz.Width, 2);
            Matrix<double> temp = new Matrix<double>(imgSz.Height, imgSz.Width, 3);
            Matrix<double> point = new Matrix<double>(3,2);
            int newX, newY;

            PointF o = new PointF((float)cameraParam[ix].cameraMatrix.Data[0, 2],
                                  (float)cameraParam[ix].cameraMatrix.Data[1, 2]);
            double fx = cameraParam[ix].cameraMatrix.Data[0, 0];
            double fy = cameraParam[ix].cameraMatrix.Data[1, 1];

            Matrix<double> rectMat;
            if (ix == 1)
            {
                Matrix<double> invR = new Matrix<double>(3, 3);
                //CvInvoke.Invert(R, invR, DecompMethod.LU);
                rectMat = R.Mul(getRectMatrix());
            }
            else
                rectMat = getRectMatrix();

            int maxX = int.MinValue, maxY = int.MinValue;
            int minX = int.MaxValue, minY = int.MaxValue;
            
            for (int y = 0; y < imgSz.Height; y++)
                for (int x = 0; x < imgSz.Width; x++)
                {
                    point[0, 0] = (x - o.X) / fx;
                    point[1, 0] = (y - o.Y) / fy;
                    point[2, 0] = -1;

                    point[0, 1] = rectMat[0, 0] * point[0, 0]
                                + rectMat[0, 1] * point[1, 0]
                                + rectMat[0, 2] * point[2, 0];

                    point[1, 1] = rectMat[1, 0] * point[0, 0]
                                + rectMat[1, 1] * point[1, 0]
                                + rectMat[1, 2] * point[2, 0];

                    point[2, 1] = rectMat[2, 0] * point[0, 0]
                                + rectMat[2, 1] * point[1, 0]
                                + rectMat[2, 2] * point[2, 0];

                    newX = (int) (point[0, 1] * fx / point[2, 1] + o.X + 0.5);
                    newY = (int) (point[1, 1] * fy / point[2, 1] + o.Y + 0.5);

                    if (newY > maxY) maxY = newY;
                    if (newY < minY) minY = newY;
                    if (newX > maxX) maxX = newX;
                    if (newX < minX) minX = newX;
                    
                    outMat.Data[y, 2 * x] = newY;
                    outMat.Data[y, 2 * x + 1] = newX;
                }

            double scale =  (double)(imgSz.Width-1) / (maxX - minX);
            //double scale = (double)(imgSz.Height-1) / (maxY - minY);

            for (int y = 0; y < imgSz.Height; y++)
                for (int x = 0; x < imgSz.Width; x++)
                {
                    outMat.Data[y, 2 * x] = (int)((outMat.Data[y, 2 * x] - minY) * scale + 0.5);
                    outMat.Data[y, 2 * x + 1] = (int)((outMat.Data[y, 2 * x + 1] - minX) * scale + 0.5);

                    if (outMat.Data[y, 2 * x + 1] >= imgSz.Width || outMat.Data[y, 2 * x + 1] < 0)
                        outMat.Data[y, 2 * x + 1] = 0;
                    if (outMat.Data[y, 2 * x] >= imgSz.Height || outMat.Data[y, 2 * x] < 0)
                        outMat.Data[y, 2 * x] = 0;
                }

            return outMat;
        }
        
        private Matrix<double> getRectMatrix()
        {            
            Matrix<double> e1 = T.Clone().Mul(1 / T.Norm);
            Matrix<double> e2 = new Matrix<double>(new double[3] { -T[1, 0], T[0, 0], 0 })
                                .Mul(1 / Math.Sqrt(T[0, 0] * T[0, 0] + T[1, 0] * T[1, 0]));
            Matrix<double> e3 = new Matrix<double>(3, 1);
            e1.Mat.Cross(e2.Mat).CopyTo(e3.Mat);
            e3.Mul(e3.Norm);

            return e1.Transpose().ConcateVertical(e2.Transpose().ConcateVertical(e3.Transpose()));
        }
        
        // we do not compute relative R and T because of minimum reprojection -> stereo calibrate
        private void composeRT()
        {
            /*
            Matrix<double> rMatLeft = new Matrix<double>(3, 3, 1);
            Matrix<double> rMatRight = new Matrix<double>(3, 3, 1);
            CvInvoke.Rodrigues(cameraParam[0].rotationMatrix.Row(0), rMatLeft.Mat);
            CvInvoke.Rodrigues(cameraParam[1].rotationMatrix.Row(0), rMatRight.Mat);

            double[] data = new double[3];
            cameraParam[0].translationMatrix.Row(0).CopyTo(data);
            Matrix<double> tMatLeft = new Matrix<double>(data);

            cameraParam[1].translationMatrix.Row(0).CopyTo(data);
            Matrix<double> tMatRight = new Matrix<double>(data);

            R = rMatRight.Mul(rMatLeft.Transpose()); // R = Rr*Rl'

            Matrix<double> U = new Matrix<double>(3, 3);
            Matrix<double> D = new Matrix<double>(3, 1);
            Matrix<double> V = new Matrix<double>(3, 3);
            CvInvoke.SVDecomp(R.Mat, D.Mat, U.Mat, V.Mat, SvdFlag.Default);
            Matrix<double> newD = new Matrix<double>(new double[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });

            R = U.Mul(newD.Mul(V.Transpose()));
            T = tMatLeft.Sub(R.Transpose().Mul(tMatRight));
            */
        }

        public void saveData(string path)
        {
            if (!_isCalibrated)
                return;
            /*
            StreamWriter file = new StreamWriter(path, false);
            for (int cam = 0; cam < cameraParam.Count; cam++)
            {
                file.WriteLine("Camera Matrix "+ cam +":");
                for (int row = 0; row < cameraParam[cam].cameraMatrix.Rows; row++)
                {
                    for (int col = 0; col < cameraParam[cam].cameraMatrix.Cols; col++)
                    {
                        file.Write("{0,10:F3}", cameraParam[cam].cameraMatrix[row, col]);
                    }
                    file.WriteLine();
                }
                file.WriteLine();

                file.WriteLine("Distortion Matrix "+ cam +":");
                for (int row = 0; row < cameraParam[cam].distortionCoeffs.Rows; row++)
                {
                    for (int col = 0; col < cameraParam[cam].distortionCoeffs.Cols; col++)
                    {
                        file.Write("{0,10:F3}", cameraParam[cam].distortionCoeffs[row, col]);
                    }
                    file.WriteLine();
                }
            }
            */
        }
    }
}
