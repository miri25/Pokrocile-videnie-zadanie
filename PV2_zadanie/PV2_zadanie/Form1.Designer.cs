namespace PV2_zadanie
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.builtWebCam_imageBox = new Emgu.CV.UI.ImageBox();
            this.captureButton = new System.Windows.Forms.Button();
            this.USBCam1_imageBox = new Emgu.CV.UI.ImageBox();
            this.label1 = new System.Windows.Forms.Label();
            this.USBCam2_imageBox = new Emgu.CV.UI.ImageBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.frame1_imageBox = new Emgu.CV.UI.ImageBox();
            this.label4 = new System.Windows.Forms.Label();
            this.frame2_imageBox = new Emgu.CV.UI.ImageBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.ButtonCalibrate = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.builtWebCam_imageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.USBCam1_imageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.USBCam2_imageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frame1_imageBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.frame2_imageBox)).BeginInit();
            this.SuspendLayout();
            // 
            // imageBox1
            // 
            this.imageBox1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.imageBox1.Location = new System.Drawing.Point(12, 394);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(100, 39);
            this.imageBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            this.imageBox1.Click += new System.EventHandler(this.imageBox1_Click);
            // 
            // builtWebCam_imageBox
            // 
            this.builtWebCam_imageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.builtWebCam_imageBox.Location = new System.Drawing.Point(12, 36);
            this.builtWebCam_imageBox.Name = "builtWebCam_imageBox";
            this.builtWebCam_imageBox.Size = new System.Drawing.Size(455, 341);
            this.builtWebCam_imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.builtWebCam_imageBox.TabIndex = 3;
            this.builtWebCam_imageBox.TabStop = false;
            // 
            // captureButton
            // 
            this.captureButton.Location = new System.Drawing.Point(216, 394);
            this.captureButton.Name = "captureButton";
            this.captureButton.Size = new System.Drawing.Size(156, 50);
            this.captureButton.TabIndex = 4;
            this.captureButton.Text = "Start capture";
            this.captureButton.UseVisualStyleBackColor = true;
            this.captureButton.Click += new System.EventHandler(this.captureButton_Click);
            // 
            // USBCam1_imageBox
            // 
            this.USBCam1_imageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.USBCam1_imageBox.Location = new System.Drawing.Point(473, 36);
            this.USBCam1_imageBox.Name = "USBCam1_imageBox";
            this.USBCam1_imageBox.Size = new System.Drawing.Size(455, 341);
            this.USBCam1_imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.USBCam1_imageBox.TabIndex = 5;
            this.USBCam1_imageBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(106, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "Built-in webcam";
            // 
            // USBCam2_imageBox
            // 
            this.USBCam2_imageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.USBCam2_imageBox.Location = new System.Drawing.Point(934, 36);
            this.USBCam2_imageBox.Name = "USBCam2_imageBox";
            this.USBCam2_imageBox.Size = new System.Drawing.Size(455, 341);
            this.USBCam2_imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.USBCam2_imageBox.TabIndex = 7;
            this.USBCam2_imageBox.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(470, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "USB webcam 1";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(931, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 17);
            this.label3.TabIndex = 9;
            this.label3.Text = "USB webcam 2";
            // 
            // frame1_imageBox
            // 
            this.frame1_imageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.frame1_imageBox.Location = new System.Drawing.Point(473, 446);
            this.frame1_imageBox.Name = "frame1_imageBox";
            this.frame1_imageBox.Size = new System.Drawing.Size(455, 341);
            this.frame1_imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.frame1_imageBox.TabIndex = 10;
            this.frame1_imageBox.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(470, 426);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(150, 17);
            this.label4.TabIndex = 11;
            this.label4.Text = "Captured frame USB 1";
            // 
            // frame2_imageBox
            // 
            this.frame2_imageBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.frame2_imageBox.Location = new System.Drawing.Point(934, 446);
            this.frame2_imageBox.Name = "frame2_imageBox";
            this.frame2_imageBox.Size = new System.Drawing.Size(455, 341);
            this.frame2_imageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.frame2_imageBox.TabIndex = 12;
            this.frame2_imageBox.TabStop = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(931, 426);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(150, 17);
            this.label5.TabIndex = 13;
            this.label5.Text = "Captured frame USB 2";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(639, 394);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(113, 36);
            this.button1.TabIndex = 14;
            this.button1.Text = "Capture 1";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(1122, 394);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(113, 36);
            this.button2.TabIndex = 15;
            this.button2.Text = "Capture 2";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // ButtonCalibrate
            // 
            this.ButtonCalibrate.Location = new System.Drawing.Point(311, 617);
            this.ButtonCalibrate.Name = "ButtonCalibrate";
            this.ButtonCalibrate.Size = new System.Drawing.Size(156, 50);
            this.ButtonCalibrate.TabIndex = 16;
            this.ButtonCalibrate.Text = "Calibrate";
            this.ButtonCalibrate.UseVisualStyleBackColor = true;
            this.ButtonCalibrate.Click += new System.EventHandler(this.ButtonCalibrate_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1437, 799);
            this.Controls.Add(this.ButtonCalibrate);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.frame2_imageBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.frame1_imageBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.USBCam2_imageBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.USBCam1_imageBox);
            this.Controls.Add(this.captureButton);
            this.Controls.Add(this.builtWebCam_imageBox);
            this.Controls.Add(this.imageBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.builtWebCam_imageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.USBCam1_imageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.USBCam2_imageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frame1_imageBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.frame2_imageBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Emgu.CV.UI.ImageBox imageBox1;
        private Emgu.CV.UI.ImageBox builtWebCam_imageBox;
        private System.Windows.Forms.Button captureButton;
        private Emgu.CV.UI.ImageBox USBCam1_imageBox;
        private System.Windows.Forms.Label label1;
        private Emgu.CV.UI.ImageBox USBCam2_imageBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private Emgu.CV.UI.ImageBox frame1_imageBox;
        private System.Windows.Forms.Label label4;
        private Emgu.CV.UI.ImageBox frame2_imageBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button ButtonCalibrate;
    }
}

