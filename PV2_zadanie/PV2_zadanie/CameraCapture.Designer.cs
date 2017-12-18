namespace PV2_zadanie
{
    partial class CameraCapture
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.buttonSelect = new System.Windows.Forms.Button();
            this.buttonSave = new System.Windows.Forms.Button();
            this.frameTimer = new System.Windows.Forms.Timer(this.components);
            this.flowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.labelSelect = new System.Windows.Forms.Label();
            this.buttonCalibration = new System.Windows.Forms.Button();
            this.buttonSaveCalib = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonSelect
            // 
            this.buttonSelect.Location = new System.Drawing.Point(1327, 43);
            this.buttonSelect.Name = "buttonSelect";
            this.buttonSelect.Size = new System.Drawing.Size(75, 23);
            this.buttonSelect.TabIndex = 1;
            this.buttonSelect.Text = "Select";
            this.buttonSelect.UseVisualStyleBackColor = true;
            this.buttonSelect.Click += new System.EventHandler(this.buttonSelect_Click);
            // 
            // buttonSave
            // 
            this.buttonSave.Enabled = false;
            this.buttonSave.Location = new System.Drawing.Point(1327, 72);
            this.buttonSave.Name = "buttonSave";
            this.buttonSave.Size = new System.Drawing.Size(75, 23);
            this.buttonSave.TabIndex = 2;
            this.buttonSave.Text = "Save";
            this.buttonSave.UseVisualStyleBackColor = true;
            this.buttonSave.Click += new System.EventHandler(this.buttonSave_Click);
            // 
            // frameTimer
            // 
            this.frameTimer.Tick += new System.EventHandler(this.frameTimer_Tick);
            // 
            // flowLayoutPanel
            // 
            this.flowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.flowLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel.Name = "flowLayoutPanel";
            this.flowLayoutPanel.Size = new System.Drawing.Size(1310, 636);
            this.flowLayoutPanel.TabIndex = 3;
            // 
            // labelSelect
            // 
            this.labelSelect.AutoSize = true;
            this.labelSelect.Location = new System.Drawing.Point(1324, 98);
            this.labelSelect.Name = "labelSelect";
            this.labelSelect.Size = new System.Drawing.Size(55, 13);
            this.labelSelect.TabIndex = 4;
            this.labelSelect.Text = "Selected: ";
            // 
            // buttonCalibration
            // 
            this.buttonCalibration.Enabled = false;
            this.buttonCalibration.Location = new System.Drawing.Point(1327, 147);
            this.buttonCalibration.Name = "buttonCalibration";
            this.buttonCalibration.Size = new System.Drawing.Size(75, 23);
            this.buttonCalibration.TabIndex = 5;
            this.buttonCalibration.Text = "Calibrate";
            this.buttonCalibration.UseVisualStyleBackColor = true;
            this.buttonCalibration.Click += new System.EventHandler(this.buttonCalibration_Click);
            // 
            // buttonSaveCalib
            // 
            this.buttonSaveCalib.Enabled = false;
            this.buttonSaveCalib.Location = new System.Drawing.Point(1327, 176);
            this.buttonSaveCalib.Name = "buttonSaveCalib";
            this.buttonSaveCalib.Size = new System.Drawing.Size(75, 23);
            this.buttonSaveCalib.TabIndex = 6;
            this.buttonSaveCalib.Text = "Save Calib.";
            this.buttonSaveCalib.UseVisualStyleBackColor = true;
            this.buttonSaveCalib.Click += new System.EventHandler(this.buttonSaveCalib_Click);
            // 
            // CameraCapture
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1414, 636);
            this.Controls.Add(this.buttonSaveCalib);
            this.Controls.Add(this.buttonCalibration);
            this.Controls.Add(this.labelSelect);
            this.Controls.Add(this.flowLayoutPanel);
            this.Controls.Add(this.buttonSave);
            this.Controls.Add(this.buttonSelect);
            this.Name = "CameraCapture";
            this.Text = "CameraCapture";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CameraCapture_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button buttonSelect;
        private System.Windows.Forms.Button buttonSave;
        private System.Windows.Forms.Timer frameTimer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel;
        private System.Windows.Forms.Label labelSelect;
        private System.Windows.Forms.Button buttonCalibration;
        private System.Windows.Forms.Button buttonSaveCalib;
    }
}
