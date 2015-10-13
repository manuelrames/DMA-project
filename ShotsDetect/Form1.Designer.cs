namespace ShotsDetect
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
            // Make sure to release the Capture object to avoid hanging
            if (m_play != null)
            {
                m_play.Dispose();
            }
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            this.btnBrowse = new System.Windows.Forms.Button();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.playTimer = new System.Windows.Forms.Timer(this.components);
            this.lableTime = new System.Windows.Forms.Label();
            this.btPDSD = new System.Windows.Forms.Button();
            this.labelFrameNumber = new System.Windows.Forms.Label();
            this.tbFrameNum = new System.Windows.Forms.TextBox();
            this.frameTime = new System.Windows.Forms.Timer(this.components);
            this.labelShotsNum = new System.Windows.Forms.Label();
            this.tbShotsNum = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(12, 16);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(93, 17);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(547, 21);
            this.tbFileName.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(12, 75);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(547, 277);
            this.panel1.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.images;
            this.btnStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnStart.Location = new System.Drawing.Point(12, 352);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(33, 32);
            this.btnStart.TabIndex = 3;
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // playTimer
            // 
            this.playTimer.Tick += new System.EventHandler(this.playTimer_Tick);
            // 
            // lableTime
            // 
            this.lableTime.AutoSize = true;
            this.lableTime.Location = new System.Drawing.Point(452, 355);
            this.lableTime.Name = "lableTime";
            this.lableTime.Size = new System.Drawing.Size(107, 12);
            this.lableTime.TabIndex = 6;
            this.lableTime.Text = "00:00:00/00:00:00";
            // 
            // btPDSD
            // 
            this.btPDSD.Location = new System.Drawing.Point(12, 46);
            this.btPDSD.Name = "btPDSD";
            this.btPDSD.Size = new System.Drawing.Size(75, 23);
            this.btPDSD.TabIndex = 7;
            this.btPDSD.Text = "PDSD";
            this.btPDSD.UseVisualStyleBackColor = true;
            this.btPDSD.Click += new System.EventHandler(this.btPDSD_Click);
            // 
            // labelFrameNumber
            // 
            this.labelFrameNumber.AutoSize = true;
            this.labelFrameNumber.Location = new System.Drawing.Point(565, 75);
            this.labelFrameNumber.Name = "labelFrameNumber";
            this.labelFrameNumber.Size = new System.Drawing.Size(59, 12);
            this.labelFrameNumber.TabIndex = 8;
            this.labelFrameNumber.Text = "Frame Num";
            // 
            // tbFrameNum
            // 
            this.tbFrameNum.Enabled = false;
            this.tbFrameNum.Location = new System.Drawing.Point(565, 90);
            this.tbFrameNum.Name = "tbFrameNum";
            this.tbFrameNum.Size = new System.Drawing.Size(119, 21);
            this.tbFrameNum.TabIndex = 9;
            // 
            // frameTime
            // 
            this.frameTime.Tick += new System.EventHandler(this.frameTime_Tick);
            // 
            // labelShotsNum
            // 
            this.labelShotsNum.AutoSize = true;
            this.labelShotsNum.Location = new System.Drawing.Point(567, 118);
            this.labelShotsNum.Name = "labelShotsNum";
            this.labelShotsNum.Size = new System.Drawing.Size(59, 12);
            this.labelShotsNum.TabIndex = 10;
            this.labelShotsNum.Text = "Shots Num";
            // 
            // tbShotsNum
            // 
            this.tbShotsNum.Enabled = false;
            this.tbShotsNum.Location = new System.Drawing.Point(565, 134);
            this.tbShotsNum.Name = "tbShotsNum";
            this.tbShotsNum.Size = new System.Drawing.Size(119, 21);
            this.tbShotsNum.TabIndex = 11;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::ShotsDetect.Properties.Resources.form1_background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(743, 396);
            this.Controls.Add(this.tbShotsNum);
            this.Controls.Add(this.labelShotsNum);
            this.Controls.Add(this.btPDSD);
            this.Controls.Add(this.tbFrameNum);
            this.Controls.Add(this.labelFrameNumber);
            this.Controls.Add(this.lableTime);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tbFileName);
            this.Controls.Add(this.btnBrowse);
            this.Name = "Form1";
            this.Text = "ShotsDetect";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Timer playTimer;
        private System.Windows.Forms.Label lableTime;
        private System.Windows.Forms.Button btPDSD;
        private System.Windows.Forms.Label labelFrameNumber;
        private System.Windows.Forms.TextBox tbFrameNum;
        private System.Windows.Forms.Timer frameTime;
        private System.Windows.Forms.Label labelShotsNum;
        private System.Windows.Forms.TextBox tbShotsNum;
    }
}


