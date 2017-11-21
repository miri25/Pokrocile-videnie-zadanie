using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System.IO;
using System.Runtime.InteropServices;
using CalcLib.Analyza.Filter;

namespace CalcLib.Analyza.Filter
{
    public class AFKalibracia : AFRoot, iaf
    {
        private Mat _cameraMatrix;     //vnutorna matica kamery
        private Mat _distortionCoeffs; //koeficienty skreslenia

        private float _zornyUholHorizont; //
        private float _zornyUholVertikal; //horizontalny a vertikalny zorny uhol kamery

        private double _sirkaSnimaca; //
        private double _vyskaSnimaca; //šírka a výška snímača kamery v [mm]

        public AFKalibracia(/*Mat cameraMatrix, Mat distortionCoeffs,*/double cm00, double cm01, double cm02, double cm10, double cm11, double cm12, double cm20, double cm21, double cm22, double d0, double d1, double d2, double d3, double d4, double sirka, double vyska)
        {
            Mat cameraMatrix = new Mat(3, 3, DepthType.Cv64F, 1);
            Mat distortionCoeffs = new Mat(5, 1, DepthType.Cv64F, 1);
            cameraMatrix.SetValue(0, 0, cm00); //2842.64422
            cameraMatrix.SetValue(0, 1, cm01); //0.00
            cameraMatrix.SetValue(0, 2, cm02); //2045.03798
            cameraMatrix.SetValue(1, 0, cm10); //0.00
            cameraMatrix.SetValue(1, 1, cm11); //2816.96523
            cameraMatrix.SetValue(1, 2, cm12); //1106.48584
            cameraMatrix.SetValue(2, 0, cm20); //0.00
            cameraMatrix.SetValue(2, 1, cm21); //0.00
            cameraMatrix.SetValue(2, 2, cm22); //1.00
            distortionCoeffs.SetValue(0, 0, d0); //-0.43715
            distortionCoeffs.SetValue(0, 1, d1); //0.19142
            distortionCoeffs.SetValue(0, 2, d2); //-0.00106
            distortionCoeffs.SetValue(0, 3, d3); //0.00268
            distortionCoeffs.SetValue(0, 4, d4); //0.00

            this._cameraMatrix = cameraMatrix;
            this._distortionCoeffs = distortionCoeffs;
            this._sirkaSnimaca = sirka;
            this._vyskaSnimaca = vyska;
        }

        public iaf Copy()
        {
            throw new NotImplementedException();
        }

        public override void Execute(List<CameraStatus> zoznamVstupov, List<CameraStatus> zoznamVystupov)
        {
            Image<Bgr, byte> _vstup = zoznamVstupov[0].imgBgr;

            Image<Bgr, byte> _vystup = new Image<Bgr, byte>(_vstup.Size);

            //úprava obrázka pomocou vstupných matíc
            CvInvoke.Undistort(_vstup, _vystup, _cameraMatrix, _distortionCoeffs); 
            //

            //počítanie zorného uhla
                //vypocet ohniskovej vzdialenosti v oboch osiach
                double _Fx = _cameraMatrix.GetValue(0, 0) / (_vstup.Width / _sirkaSnimaca); //ohniskova vzdialenost v osi x
                double _Fy = _cameraMatrix.GetValue(1, 1) / (_vstup.Height / _vyskaSnimaca); //ohniskova vzdialenost v osi y
                //

                //vypocet zorneho uhla kamery (vodorovne a zvislo)
                double _hfov = 2 * Math.Atan(_sirkaSnimaca / (2 * _Fx)) * (180 / Math.PI); //horizontalny uhol
                double _vfov = 2 * Math.Atan(_vyskaSnimaca / (2 * _Fy)) * (180 / Math.PI); //vertikalny uhol
                //  
            //

            CameraStatus cs = (CameraStatus)zoznamVstupov[0].Clone();
            cs.imgBgr = _vystup;

            cs.sirkaSnimaca = _sirkaSnimaca; //
            cs.vyskaSnimaca = _vyskaSnimaca; //nastavenie sirky a vysky snimaca kamery v CameraStatus

            cs.angleHorizontal = _hfov; //
            cs.angleVertical = _vfov;   //nastavenie horizontalneho a vertikalneho uhla v CameraStatus

            zoznamVystupov.Add(cs);
        }
    }
}
