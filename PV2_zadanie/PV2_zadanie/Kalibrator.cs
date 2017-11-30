using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.Util;
using Emgu.Util.TypeEnum;
using System.Threading;
using System.Xml.Serialization;
using System.IO;
using System.Drawing.Drawing2D;

namespace PV2_zadanie
{
    public class Kalibrator
    {
        //vstupné body: body reálneho sveta
        List<MCvPoint3D32f[]> ObjectPoints;

        //zoznam vnútorných rohov šachovnice - zoznam 2D bodov
        List<PointF[]> ImagePoints; //PointF[]

        //vnutorne parametre kamery
        //IntrinsicCameraParameters IntrinsicParams;
         //VectorOfPointF

        //vonkajsie parametre kamery
        Mat[] rotationVectors;
        Mat[] translationVectors; 

        //nova matica kamery(???)
        Mat newCameraMatrix; //IInputOutputArray

        //nájdené rohy v obrázku
        //IOutputArray imageCorners;

        //špecifikácia ukončenia kalibrácie
        CalibType Flag;

        //mapy použité v úprave obrázka
        Image<Gray, float> Map1, Map2;

        bool MustInitUndistort;

        private List<string> _fileList;
        private Size _boardSize;
        public Mat cameraMatrix;
        public Mat distortionCoeffs;

        public Kalibrator(List<string> fileList, Size boardSize)
        {
            _fileList = new List<string>();
            for (int i = 0; i < fileList.Count; i++)
            {
                this._fileList.Add(fileList[i]);
            }
            this._boardSize = boardSize;

            this.Flag = CalibType.Default;
            this.MustInitUndistort = true;
            this.ImagePoints = new List<PointF[]>(); //PointF[]
            this.ObjectPoints = new List<MCvPoint3D32f[]>();

            int v = AddChessboardPoints(this._fileList, this._boardSize);
            Size imsize = new Image<Bgr, byte>(this._fileList[0]).Size;
            double result = Calibrate(imsize);
        }

        /// <summary>
        /// Otvorí obrázky s šachovnicami a extrahuje rohové body
        /// </summary>
        /// <param name="fileList">zoznam mien obrázkov s šachovnicami</param>
        /// <param name="boardSize">počet vnútorných rohov šachovnice (x-1, y-1)</param>
        /// <returns></returns>
        private int AddChessboardPoints(List<string> fileList, Size boardSize)
        {
            //PointF[][] imageCorners = new PointF[Frame_array_buffer.Length][];
            //body na šachovnici
            //PointF[] imageCorners;
            //Emgu.CV.IOutputArray imageCorners;

            //poloha rohov šachovnice v 3D priestore
            MCvPoint3D32f[] objectCorners = new MCvPoint3D32f[boardSize.Height * boardSize.Width];

            //3D Scene Points:
            //Inicializácia vnútorných rohov šachovnice v 3D priestore (x,y,z) = (i,j,0)
            for (int i = 0; i < boardSize.Height; i++)
            {
                for (int j = 0; j < boardSize.Width; j++)
                {
                    objectCorners[i * boardSize.Width + j] = new MCvPoint3D32f(i, j, 0.0f);
                }
            }

            //2D body obrázka:
            Image<Gray, Byte> image;//obrázok pre načítavanie obrázka so šachovnicou
            int successes = 0; //počet najdenych obrazkov so sachovnicou
            //List<VectorOfPointF> corners = new List<VectorOfPointF>();
            GC.Collect();
            //pre všetky vstupné obrázky - uhly pohľadu
            for (int i = 0; i < fileList.Count; i++) 
            {
                var cornerPoints = new VectorOfPointF(); //vektor rohových bodov šachovnice
                image = new Image<Gray, Byte>(fileList[i]); //načítaj obrázok zo zoznamu
                //imageCorners = null; //CameraCalibration.FindChessboardCorners(image, boardSize, CALIB_CB_TYPE.DEFAULT);
                CvInvoke.FindChessboardCorners(image, boardSize, cornerPoints, CalibCbType.Default); //získaj rohové body šachovnice

                if (cornerPoints == null) continue; //keď v aktuálnom obrázku nenašiel žiadne body, zoberie ďalší    //imageCorners
                                                    //corners.Add(cornerPoints);

                //image.FindCornerSubPix( imageCorners, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

                //získaj rohové body so subpixelovou presnosťou
                CvInvoke.CornerSubPix(image, cornerPoints, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.1));

                //CvInvoke.cvFindCornerSubPix(image, imageCorners,
                //    boardSize.Height * boardSize.Width,
                //    new Size(5, 5), new Size(-1, -1), 
                //    new MCvTermCriteria(30, 0.1));

                //keď našiel na obrázku dosť bodov (9*6), tak ich pridá do zoznamu
                if (cornerPoints.Size == boardSize.Height * boardSize.Width)  //imageCorners.Length
                {
                    //zavolá metódu na pridanie bodov do zoznamov
                    AddPoints(cornerPoints.ToArray(), objectCorners);
                    successes++;
                }
            }
            return successes;
        }

        /// <summary>
        /// Metóda na pridávanie 2D a 3D bodov šachonvice do zoznamu
        /// </summary>
        /// <param name="imageCorners">2D obrazové body z jedného obrázka</param>
        /// <param name="objectCorners">korešpondujúce 3D body</param>
        void AddPoints(PointF[] imageCorners, MCvPoint3D32f[] objectCorners) //PointF[] imageCorners
        {
            this.ImagePoints.Add(imageCorners); //2D obrazové body z jedného obrázka
            this.ObjectPoints.Add(objectCorners); //korešpondujúce 3D body
        }

        /// <summary>
        /// Kalibrácia kamery
        /// </summary>
        /// <param name="imageSize">veľkosť obrázka</param>
        /// <returns>vracia chybu re-projekcie</returns>
        private double Calibrate(Size imageSize)
        {
            //undistorter must be reinitialized???
            this.MustInitUndistort = true;

            MCvPoint3D32f[][] objectPoints = this.ObjectPoints.ToArray();   
            PointF[][] imagePoints = this.ImagePoints.ToArray();// ToArray();
                                                                //VectorOfPoint objectPoints = this.ObjectPoints.ToArray();
                                                                //VectorOfPoint[] imagePoints = this.ImagePoints.ToArray();

            cameraMatrix = new Mat(3, 3, DepthType.Cv64F, 1); 
            distortionCoeffs = new Mat(8, 1, DepthType.Cv64F, 1);
            //rotationVectors = new Mat[1];
            //translationVectors = new Mat[1];

            //this.IntrinsicParams = new IntrinsicCameraParameters();
            //ExtrinsicCameraParameters[] extrinsicParams; 
            
            //najdenie matic vnutornych a vonkajsich parametrov kamery
            double vysledok = CvInvoke.CalibrateCamera( objectPoints, imagePoints, imageSize, cameraMatrix, distortionCoeffs, CalibType.Default, new MCvTermCriteria(30, 0.1), out rotationVectors, out translationVectors);
            return vysledok;
        }

        ///// <summary>
        ///// odstráni skreslenie z obrázka (po kalibrácii)
        ///// </summary>
        ///// <param name="image"></param>
        ///// <returns></returns>
        //public Image<Gray, Byte> Remap(Image<Gray, Byte> image, int metoda)
        //{
        //    Mat cameraMatrix2 = new Mat(3, 3, DepthType.Cv64F, 1);
        //    Mat distortionCoeffs2 = new Mat(5, 1, DepthType.Cv64F, 1);
        //    cameraMatrix2.SetValue(0, 0, 2842.64422);
        //    cameraMatrix2.SetValue(0, 1, 0.00);
        //    cameraMatrix2.SetValue(0, 2, 2045.03798);
        //    cameraMatrix2.SetValue(1, 0, 0.00);
        //    cameraMatrix2.SetValue(1, 1, 2816.96523);
        //    cameraMatrix2.SetValue(1, 2, 1106.48584);
        //    cameraMatrix2.SetValue(2, 0, 0.00);
        //    cameraMatrix2.SetValue(2, 1, 0.00);
        //    cameraMatrix2.SetValue(2, 2, 1.00);
        //    distortionCoeffs2.SetValue(0, 0, -0.43715);
        //    distortionCoeffs2.SetValue(0, 1, 0.19142);
        //    distortionCoeffs2.SetValue(0, 2, -0.00106);
        //    distortionCoeffs2.SetValue(0, 3, 0.00268);
        //    distortionCoeffs2.SetValue(0, 4, 0.00);
        //    Image<Gray, Byte> undistorted = new Image<Gray, byte>(image.Size); //obrázok pre uloženie upraveného obrázka
        //    if (metoda == 1)
        //    {
        //        if (this.MustInitUndistort)//called once per calibration
        //        {
        //            this.Map1 = new Image<Gray, float>(image.Size);
        //            this.Map2 = new Image<Gray, float>(image.Size);
        //            IInputArray R = null; //???????
        //            newCameraMatrix = new Mat(3, 3, DepthType.Cv64F, 1);

        //            CvInvoke.InitUndistortRectifyMap(cameraMatrix2, distortionCoeffs2, R, newCameraMatrix, undistorted.Size, DepthType.Cv32F, Map1, Map2);

        //            //XML serializácia objektov Map1/2 do textových súborov
        //            XmlSerializer serializer = new XmlSerializer(typeof(Image<Gray, float>));
        //            TextWriter textWriter = new StreamWriter("Map1.txt");
        //            serializer.Serialize(textWriter, Map1);
        //            textWriter.Close();
        //            TextWriter textWriter2 = new StreamWriter("Map2.txt");
        //            serializer.Serialize(textWriter2, Map2);
        //            textWriter2.Close();
        //            //

        //            this.MustInitUndistort = false;
        //        }
        //        //premapovanie obrázka
        //        CvInvoke.Remap(image, undistorted, Map1, Map2, Inter.Linear, BorderType.Constant, new MCvScalar(0)); 
        //    }
        //    else if (metoda == 2) {
        //        //XmlSerializer serializer = new XmlSerializer(typeof(MatDataAllocator));
        //        //TextWriter textWriter = new StreamWriter("cameraMatrix.txt");
        //        //serializer.Serialize(textWriter, cameraMatrix.Data);
        //        //textWriter.Close();
        //        //TextWriter textWriter2 = new StreamWriter("distortionCoeffs.txt");
        //        //serializer.Serialize(textWriter2, distortionCoeffs.Data);
        //        //textWriter2.Close();
                //CvInvoke.Undistort(image, undistorted, cameraMatrix2, distortionCoeffs2);  //cameraMatrix, distortionCoeffs
        //    }
        //    return undistorted;
        //}
    }
}
