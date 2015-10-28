namespace ShotsDetect
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
            this.components = new System.ComponentModel.Container();
            this.tbP1 = new System.Windows.Forms.TextBox();
            this.tbP2 = new System.Windows.Forms.TextBox();
            this.p1 = new System.Windows.Forms.Label();
            this.p2 = new System.Windows.Forms.Label();
            this.p1_expl = new System.Windows.Forms.Label();
            this.p2_expl = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.cbAlgorithm = new System.Windows.Forms.ComboBox();
            this.alg_expl = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbFrameNum = new System.Windows.Forms.TextBox();
            this.tbShotsNum = new System.Windows.Forms.TextBox();
            this.frameTime = new System.Windows.Forms.Timer(this.components);
            this.bLoad = new System.Windows.Forms.Button();
            this.tbTime = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // tbP1
            // 
            this.tbP1.Location = new System.Drawing.Point(114, 82);
            this.tbP1.Name = "tbP1";
            this.tbP1.Size = new System.Drawing.Size(100, 21);
            this.tbP1.TabIndex = 0;
            // 
            // tbP2
            // 
            this.tbP2.Location = new System.Drawing.Point(114, 126);
            this.tbP2.Name = "tbP2";
            this.tbP2.Size = new System.Drawing.Size(100, 21);
            this.tbP2.TabIndex = 1;
            // 
            // p1
            // 
            this.p1.AutoSize = true;
            this.p1.Location = new System.Drawing.Point(24, 85);
            this.p1.Name = "p1";
            this.p1.Size = new System.Drawing.Size(65, 12);
            this.p1.TabIndex = 2;
            this.p1.Text = "parameter1";
            // 
            // p2
            // 
            this.p2.AutoSize = true;
            this.p2.Location = new System.Drawing.Point(24, 129);
            this.p2.Name = "p2";
            this.p2.Size = new System.Drawing.Size(65, 12);
            this.p2.TabIndex = 3;
            this.p2.Text = "parameter2";
            // 
            // p1_expl
            // 
            this.p1_expl.AutoSize = true;
            this.p1_expl.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p1_expl.Location = new System.Drawing.Point(34, 240);
            this.p1_expl.Name = "p1_expl";
            this.p1_expl.Size = new System.Drawing.Size(117, 13);
            this.p1_expl.TabIndex = 4;
            this.p1_expl.Text = "explanation parameter1";
            // 
            // p2_expl
            // 
            this.p2_expl.AutoSize = true;
            this.p2_expl.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.p2_expl.Location = new System.Drawing.Point(34, 280);
            this.p2_expl.Name = "p2_expl";
            this.p2_expl.Size = new System.Drawing.Size(117, 13);
            this.p2_expl.TabIndex = 5;
            this.p2_expl.Text = "explanation parameter2";
            this.p2_expl.Click += new System.EventHandler(this.label4_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(34, 34);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "Algorithm:";
            // 
            // cbAlgorithm
            // 
            this.cbAlgorithm.FormattingEnabled = true;
            this.cbAlgorithm.Location = new System.Drawing.Point(105, 31);
            this.cbAlgorithm.Name = "cbAlgorithm";
            this.cbAlgorithm.Size = new System.Drawing.Size(121, 20);
            this.cbAlgorithm.TabIndex = 7;
            this.cbAlgorithm.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // alg_expl
            // 
            this.alg_expl.AutoSize = true;
            this.alg_expl.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.alg_expl.Location = new System.Drawing.Point(34, 197);
            this.alg_expl.Name = "alg_expl";
            this.alg_expl.Size = new System.Drawing.Size(106, 13);
            this.alg_expl.TabIndex = 8;
            this.alg_expl.Text = "explanation algorithm";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(124, 162);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 21);
            this.button1.TabIndex = 9;
            this.button1.Text = "Execute";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(257, 85);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 10;
            this.label1.Text = "# frames";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(257, 126);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 11;
            this.label2.Text = "# shots";
            // 
            // tbFrameNum
            // 
            this.tbFrameNum.Enabled = false;
            this.tbFrameNum.Location = new System.Drawing.Point(324, 82);
            this.tbFrameNum.Name = "tbFrameNum";
            this.tbFrameNum.Size = new System.Drawing.Size(100, 21);
            this.tbFrameNum.TabIndex = 12;
            // 
            // tbShotsNum
            // 
            this.tbShotsNum.Enabled = false;
            this.tbShotsNum.Location = new System.Drawing.Point(324, 126);
            this.tbShotsNum.Name = "tbShotsNum";
            this.tbShotsNum.Size = new System.Drawing.Size(100, 21);
            this.tbShotsNum.TabIndex = 13;
            // 
            // frameTime
            // 
            this.frameTime.Tick += new System.EventHandler(this.frameTime_Tick);
            // 
            // bLoad
            // 
            this.bLoad.Location = new System.Drawing.Point(335, 162);
            this.bLoad.Name = "bLoad";
            this.bLoad.Size = new System.Drawing.Size(75, 21);
            this.bLoad.TabIndex = 14;
            this.bLoad.Text = "Load";
            this.bLoad.UseVisualStyleBackColor = true;
            this.bLoad.Click += new System.EventHandler(this.bLoad_Click);
            // 
            // tbTime
            // 
            this.tbTime.Enabled = false;
            this.tbTime.Location = new System.Drawing.Point(324, 49);
            this.tbTime.Name = "tbTime";
            this.tbTime.Size = new System.Drawing.Size(100, 21);
            this.tbTime.TabIndex = 15;
            this.tbTime.Text = "00:00:00";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(275, 52);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 16;
            this.label3.Text = "time";
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(486, 332);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbTime);
            this.Controls.Add(this.bLoad);
            this.Controls.Add(this.tbShotsNum);
            this.Controls.Add(this.tbFrameNum);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.alg_expl);
            this.Controls.Add(this.cbAlgorithm);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.p2_expl);
            this.Controls.Add(this.p1_expl);
            this.Controls.Add(this.p2);
            this.Controls.Add(this.p1);
            this.Controls.Add(this.tbP2);
            this.Controls.Add(this.tbP1);
            this.Name = "Form2";
            this.Text = "Form2";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbP1;
        private System.Windows.Forms.TextBox tbP2;
        private System.Windows.Forms.Label p1;
        private System.Windows.Forms.Label p2;
        private System.Windows.Forms.Label p1_expl;
        private System.Windows.Forms.Label p2_expl;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cbAlgorithm;
        private System.Windows.Forms.Label alg_expl;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbFrameNum;
        private System.Windows.Forms.TextBox tbShotsNum;
        private System.Windows.Forms.Timer frameTime;
        private System.Windows.Forms.Button bLoad;
        private System.Windows.Forms.TextBox tbTime;
        private System.Windows.Forms.Label label3;
    }
}