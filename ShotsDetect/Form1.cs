using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ShotsDetect
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        enum State
        {
            Uninit,
            Stopped,
            Paused,
            Playing
        }
        State m_State = State.Uninit;
        Capture m_play = null;

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
                tbFileName.Text = ofd.FileName;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // If we have no file open
            if (m_play == null)
            {
                // Open the file, provide a handle to play it in
                m_play = new Capture(panel1, tbFileName.Text);

                // Let us know when the file is finished playing
                //m_play.StopPlay += new Program.DxPlayEvent(m_play_StopPlay);
                //m_State = State.Stopped
            }

            //need modify
            btnStart.Text = "Stop";
            m_play.Start();
            btnPause.Enabled = true;
            tbFileName.Enabled = false;
            m_State = State.Playing;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {

        }
    }
}
