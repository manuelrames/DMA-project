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
            {
                tbFileName.Text = ofd.FileName;
                m_play = new Capture(panel1, tbFileName.Text);
                m_State = State.Stopped;
                tbFileName.Enabled = false;
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            // Only do something if a video is loaded
            if (m_play != null)
            {
                // Depending on the state you have to do a different action
                switch (m_State) { 
                    case State.Stopped:
                        m_play.Start();
                        m_State = State.Playing;
                        this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.pause;
                        break;
                    case State.Playing:
                        m_play.Stop();
                        m_State = State.Paused;
                        this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.images;
                        break;
                    case State.Paused:
                        m_play.Start();
                        m_State = State.Playing;
                        this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.pause;
                        break;
            }
            }
        }

    }
}
