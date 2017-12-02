namespace PV2_zadanie
{
    partial class Form2
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
            this.buttonImgCapture = new System.Windows.Forms.Button();
            this.buttonCalibration = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonImgCapture
            // 
            this.buttonImgCapture.Location = new System.Drawing.Point(12, 12);
            this.buttonImgCapture.Name = "buttonImgCapture";
            this.buttonImgCapture.Size = new System.Drawing.Size(225, 35);
            this.buttonImgCapture.TabIndex = 0;
            this.buttonImgCapture.Text = "CaptureImages";
            this.buttonImgCapture.UseVisualStyleBackColor = true;
            this.buttonImgCapture.Click += new System.EventHandler(this.buttonImgCapture_Click);
            // 
            // buttonCalibration
            // 
            this.buttonCalibration.Location = new System.Drawing.Point(12, 53);
            this.buttonCalibration.Name = "buttonCalibration";
            this.buttonCalibration.Size = new System.Drawing.Size(225, 35);
            this.buttonCalibration.TabIndex = 1;
            this.buttonCalibration.Text = "Calibrate";
            this.buttonCalibration.UseVisualStyleBackColor = true;
            this.buttonCalibration.Click += new System.EventHandler(this.buttonCalibration_Click);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(249, 425);
            this.Controls.Add(this.buttonCalibration);
            this.Controls.Add(this.buttonImgCapture);
            this.Name = "Form2";
            this.Text = "Form2";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonImgCapture;
        private System.Windows.Forms.Button buttonCalibration;
    }
}