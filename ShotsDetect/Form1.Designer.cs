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
                m_play = null;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnBrowse = new System.Windows.Forms.Button();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnStart = new System.Windows.Forms.Button();
            this.playTimer = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.shotDetectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.labelTime = new System.Windows.Forms.Label();
            this.lbPlay = new System.Windows.Forms.ListBox();
            this.tbTags = new System.Windows.Forms.TextBox();
            this.btAdd = new System.Windows.Forms.Button();
            this.btExport = new System.Windows.Forms.Button();
            this.lbTags = new System.Windows.Forms.ListBox();
            this.btDel = new System.Windows.Forms.Button();
            this.bFrame = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.bRetrieveShots = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnBrowse
            // 
            this.btnBrowse.Location = new System.Drawing.Point(31, 50);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(75, 25);
            this.btnBrowse.TabIndex = 0;
            this.btnBrowse.Text = "Browse";
            this.btnBrowse.UseVisualStyleBackColor = true;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);
            // 
            // tbFileName
            // 
            this.tbFileName.Location = new System.Drawing.Point(150, 50);
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.Size = new System.Drawing.Size(522, 20);
            this.tbFileName.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(150, 76);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(522, 250);
            this.panel1.TabIndex = 2;
            // 
            // btnStart
            // 
            this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.images;
            this.btnStart.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.btnStart.Location = new System.Drawing.Point(371, 329);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(24, 24);
            this.btnStart.TabIndex = 3;
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // playTimer
            // 
            this.playTimer.Tick += new System.EventHandler(this.playTimer_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.shotDetectionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(890, 24);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // shotDetectionToolStripMenuItem
            // 
            this.shotDetectionToolStripMenuItem.Name = "shotDetectionToolStripMenuItem";
            this.shotDetectionToolStripMenuItem.Size = new System.Drawing.Size(97, 20);
            this.shotDetectionToolStripMenuItem.Text = "Shot Detection";
            this.shotDetectionToolStripMenuItem.Click += new System.EventHandler(this.shotDetectionToolStripMenuItem_Click);
            // 
            // labelTime
            // 
            this.labelTime.AutoSize = true;
            this.labelTime.Location = new System.Drawing.Point(533, 329);
            this.labelTime.Name = "labelTime";
            this.labelTime.Size = new System.Drawing.Size(96, 13);
            this.labelTime.TabIndex = 7;
            this.labelTime.Text = "00:00:00/00:00:00";
            // 
            // lbPlay
            // 
            this.lbPlay.FormattingEnabled = true;
            this.lbPlay.Location = new System.Drawing.Point(150, 354);
            this.lbPlay.Name = "lbPlay";
            this.lbPlay.Size = new System.Drawing.Size(522, 95);
            this.lbPlay.TabIndex = 8;
            this.lbPlay.SelectedIndexChanged += new System.EventHandler(this.lbPlay_SelectedIndexChanged);
            // 
            // tbTags
            // 
            this.tbTags.Location = new System.Drawing.Point(678, 50);
            this.tbTags.Name = "tbTags";
            this.tbTags.Size = new System.Drawing.Size(200, 20);
            this.tbTags.TabIndex = 9;
            // 
            // btAdd
            // 
            this.btAdd.Location = new System.Drawing.Point(678, 76);
            this.btAdd.Name = "btAdd";
            this.btAdd.Size = new System.Drawing.Size(93, 25);
            this.btAdd.TabIndex = 10;
            this.btAdd.Text = "add";
            this.btAdd.UseVisualStyleBackColor = true;
            this.btAdd.Click += new System.EventHandler(this.btAdd_Click);
            // 
            // btExport
            // 
            this.btExport.Location = new System.Drawing.Point(777, 415);
            this.btExport.Name = "btExport";
            this.btExport.Size = new System.Drawing.Size(101, 34);
            this.btExport.TabIndex = 12;
            this.btExport.Text = "Export to XML";
            this.btExport.UseVisualStyleBackColor = true;
            this.btExport.Click += new System.EventHandler(this.btExport_Click);
            // 
            // lbTags
            // 
            this.lbTags.FormattingEnabled = true;
            this.lbTags.Location = new System.Drawing.Point(678, 146);
            this.lbTags.Name = "lbTags";
            this.lbTags.Size = new System.Drawing.Size(200, 251);
            this.lbTags.TabIndex = 13;
            // 
            // btDel
            // 
            this.btDel.Location = new System.Drawing.Point(787, 76);
            this.btDel.Name = "btDel";
            this.btDel.Size = new System.Drawing.Size(91, 25);
            this.btDel.TabIndex = 14;
            this.btDel.Text = "delete";
            this.btDel.UseVisualStyleBackColor = true;
            this.btDel.Click += new System.EventHandler(this.btDel_Click);
            // 
            // bFrame
            // 
            this.bFrame.Location = new System.Drawing.Point(678, 415);
            this.bFrame.Name = "bFrame";
            this.bFrame.Size = new System.Drawing.Size(93, 34);
            this.bFrame.TabIndex = 15;
            this.bFrame.Text = "Export Frame";
            this.bFrame.UseVisualStyleBackColor = true;
            this.bFrame.Click += new System.EventHandler(this.bFrame_Click);
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.DefaultExt = "jpg";
            // 
            // bRetrieveShots
            // 
            this.bRetrieveShots.Location = new System.Drawing.Point(678, 107);
            this.bRetrieveShots.Name = "bRetrieveShots";
            this.bRetrieveShots.Size = new System.Drawing.Size(200, 23);
            this.bRetrieveShots.TabIndex = 16;
            this.bRetrieveShots.Text = "Retrieve Shots";
            this.bRetrieveShots.UseVisualStyleBackColor = true;
            this.bRetrieveShots.Click += new System.EventHandler(this.bRetrieveShots_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Image = global::ShotsDetect.Properties.Resources.form1_background;
            this.label1.Location = new System.Drawing.Point(12, 126);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(121, 154);
            this.label1.TabIndex = 17;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::ShotsDetect.Properties.Resources.form1_background;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(890, 462);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.bRetrieveShots);
            this.Controls.Add(this.bFrame);
            this.Controls.Add(this.btDel);
            this.Controls.Add(this.lbTags);
            this.Controls.Add(this.btExport);
            this.Controls.Add(this.btAdd);
            this.Controls.Add(this.tbTags);
            this.Controls.Add(this.lbPlay);
            this.Controls.Add(this.labelTime);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.tbFileName);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Aorta";
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Timer playTimer;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem shotDetectionToolStripMenuItem;
        private System.Windows.Forms.Label labelTime;
        private System.Windows.Forms.ListBox lbPlay;
        private System.Windows.Forms.TextBox tbTags;
        private System.Windows.Forms.Button btAdd;
        private System.Windows.Forms.Button btExport;
        private System.Windows.Forms.ListBox lbTags;
        private System.Windows.Forms.Button btDel;
        private System.Windows.Forms.Button bFrame;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.Button bRetrieveShots;
        private System.Windows.Forms.Label label1;
    }
}


