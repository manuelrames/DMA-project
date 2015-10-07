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
            this.tbToltalTime = new System.Windows.Forms.TextBox();
            this.tbCurrentTime = new System.Windows.Forms.TextBox();
            this.playTimer = new System.Windows.Forms.Timer(this.components);
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
            this.panel1.Location = new System.Drawing.Point(93, 50);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(547, 251);
            this.panel1.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.images;
            this.btnStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnStart.Location = new System.Drawing.Point(54, 159);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(33, 32);
            this.btnStart.TabIndex = 3;
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // tbToltalTime
            // 
            this.tbToltalTime.BackColor = System.Drawing.SystemColors.Window;
            this.tbToltalTime.Enabled = false;
            this.tbToltalTime.Location = new System.Drawing.Point(580, 303);
            this.tbToltalTime.Name = "tbToltalTime";
            this.tbToltalTime.Size = new System.Drawing.Size(60, 21);
            this.tbToltalTime.TabIndex = 4;
            this.tbToltalTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // tbCurrentTime
            // 
            this.tbCurrentTime.BackColor = System.Drawing.SystemColors.Window;
            this.tbCurrentTime.Enabled = false;
            this.tbCurrentTime.Location = new System.Drawing.Point(519, 303);
            this.tbCurrentTime.Name = "tbCurrentTime";
            this.tbCurrentTime.Size = new System.Drawing.Size(60, 21);
            this.tbCurrentTime.TabIndex = 5;
            this.tbCurrentTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // playTimer
            // 
            this.playTimer.Tick += new System.EventHandler(this.playTimer_Tick);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::ShotsDetect.Properties.Resources.form1_background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(692, 331);
            this.Controls.Add(this.tbCurrentTime);
            this.Controls.Add(this.tbToltalTime);
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
        private System.Windows.Forms.TextBox tbToltalTime;
        private System.Windows.Forms.TextBox tbCurrentTime;
        private System.Windows.Forms.Timer playTimer;
    }
}


