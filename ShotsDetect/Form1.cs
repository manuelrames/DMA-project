using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
        double m_time;
        double m_position;

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbFileName.Text = ofd.FileName;
                m_play = new Capture(panel1, tbFileName.Text);
                tbFileName.Enabled = false;

                // Let us know when the file is finished playing
                m_play.StopPlay += new Capture.CaptureEvent(m_play_StopPlay);
                m_State = State.Stopped;

                ShowTime();
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
                        playTimer.Start();
                        break;
                    case State.Playing:
                        m_play.Stop();
                        m_State = State.Paused;
                        this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.images;
                        playTimer.Stop();
                        break;
                    case State.Paused:
                        m_play.Start();
                        m_State = State.Playing;
                        this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.pause;
                        playTimer.Start();
                        break;
                }
            }
        }

        private void ShowTime()
        {
            m_play.getTime(out m_position, out m_time);
            int s = (int)m_time;
            int h = s / 3600;
            int m = (s - (h * 3600)) / 60;
            s = s - (h * 3600 + m * 60);
            tbToltalTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);

            s = (int)m_position;
            h = s / 3600;
            m = (s - (h * 3600)) / 60;
            s = s - (h * 3600 + m * 60);
            tbCurrentTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);
        }

        // Called when the video is finished playing
        private void m_play_StopPlay(Object sender)
        {
            // This isn't the right way to do this, but heck, it's only a sample
            CheckForIllegalCrossThreadCalls = false;

            tbFileName.Enabled = true;
            this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.images;

            CheckForIllegalCrossThreadCalls = true;

            m_State = State.Stopped;

            // Rewind clip to beginning to allow DxPlay.Start to work again.
            m_play.Rewind();

            //playTimer.Dispose();
        }

        private void playTimer_Tick(object sender, EventArgs e)
        {
            ShowTime();
        }
    }
}
