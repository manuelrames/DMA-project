﻿using System;
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

        /// <summary>
        /// use to store shot informaiton for exporting xml file
        /// </summary>
        public struct ShotInfo
        {
            public Shot shot;
            public List<String> tags;
        }

        State m_State = State.Uninit;
        Capture m_play = null;

        double m_time;
        double m_position;

        /// <summary>
        /// duration of the media stream
        /// </summary>
        public double duration;

        /// <summary>
        /// saving all the shots produced in the session => are in the listbox lbPlay
        /// </summary>
        public List<Shot> lbShots;
        ShotInfo[] m_ShotInfo = null;

        public List<Shot> allShots;
        ShotInfo[] all_ShotInfo = null;

        public int algorithm;
        public double parameter1;
        public double parameter2;

        /// <summary>
        /// Browsing to the video file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnBrowse_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                tbFileName.Text = ofd.FileName;

                if (m_play != null)
                {
                    this.btnStart.BackgroundImage = global::ShotsDetect.Properties.Resources.images;

                    m_play.Dispose();
                    lbPlay.Items.Clear();
                }
                try
                {
                    m_play = new Capture(panel1, tbFileName.Text);
                    tbFileName.Enabled = false;

                    char[] sep = { '\\' };
                    String[] list = tbFileName.Text.Split(sep);
                    lbPlay.Items.Add(list[list.Length - 1]);

                    m_play.getMediaPosition().get_StopTime(out duration);

                    // Let us know when the file is finished playing
                    m_play.StopPlay += new Capture.CaptureEvent(m_play_StopPlay);
                    m_State = State.Stopped;

                    ShowTime();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Load file failed!");
                    tbFileName.Text = "";
                }
            }
        }

        /// <summary>
        /// Play-pause logic
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Show the video play time
        /// </summary>
        private void ShowTime()
        {
            m_play.getTime(out m_position, out m_time);
            int s = (int)m_time;
            int h = s / 3600;
            int m = (s - (h * 3600)) / 60;
            s = s - (h * 3600 + m * 60);

            int s2 = (int)m_position;
            int h2 = s2 / 3600;
            int m2 = (s2 - (h2 * 3600)) / 60;
            s2 = s2 - (h2 * 3600 + m2 * 60);
            labelTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}/{3:D2}:{4:D2}:{5:D2}", h2, m2, s2, h, m, s);
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

        // Go to shot detection form
        private void shotDetectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tbFileName.Text.Equals(""))
                MessageBox.Show("First a file must be loaded. Click Browse.");
            else
            {
                Form2 algo_form = new Form2(this, tbFileName.Text);
                algo_form.Show();
            }
        }

        public void updateLbPlay(List<Shot> shots)
        {
            while (lbPlay.Items.Count > 1)
                lbPlay.Items.RemoveAt(lbPlay.Items.Count - 1);

            if (m_ShotInfo != null)
                m_ShotInfo = null;

            if (all_ShotInfo != null)
                all_ShotInfo = null;
            allShots = new List<Shot>(shots);
            all_ShotInfo = new ShotInfo[shots.Count];


            lbShots = new List<Shot>(shots);
            m_ShotInfo = new ShotInfo[shots.Count];

            for (int i = 0; i < shots.Count; i++)
            {
                Shot s = shots[i];
                lbPlay.Items.Add(string.Format("Shot{0}:{1:N}-{2:N}  Frame #{3}-> #{4}", i+1, s.start, s.end, s.frame1, s.frame2));
                lbShots.Add(s);

                //store the shot to ShotInfo for exporting xml file
                m_ShotInfo[i].shot = s;

                allShots.Add(s);
                all_ShotInfo[i].shot = s;
                all_ShotInfo[i].tags = new List<string>();

            }
        }

        /// <summary>
        /// selects the shot
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lbPlay_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = lbPlay.SelectedIndex;
            if (index > 0)
            {
                m_play.setStartTime(lbShots[index - 1].start);
                m_play.setEndTime(lbShots[index - 1].end);

                m_play.setStartPosition(lbShots[index - 1].start);

                /* update the tags listbox info */
                lbTags.Items.Clear();
                if (m_ShotInfo[index - 1].tags != null)
                {
                    /* add already existing tags to listbox */
                    for (int i = 0; i < m_ShotInfo[index - 1].tags.Count; i++)
                    {
                        lbTags.Items.Add(m_ShotInfo[index - 1].tags[i]);
                    }
                }
            }
            else
            {
                m_play.setStartTime(0.0);
                m_play.setEndTime(duration);
                m_play.setStartPosition(0.0);
                lbTags.Items.Clear();
            }
        }

        private void btExport_Click(object sender, EventArgs e)
        {
            if (m_play != null)
            {
                string videoName;
                ShotsXml xml = null;
                var sfd = new SaveFileDialog();

                char[] sep = { '\\' };
                String[] list = tbFileName.Text.Split(sep);

                sfd.Filter = "xml files (*.xml)|*.xml|All files (*.*)|*.*";
                sfd.Title = "Save an xml file";

                /* get the video name */
                videoName = list[list.Length - 1];

                /* set the default file name */
                sfd.FileName = videoName.Substring(0, videoName.Length - 4);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    if (sfd.FileName != "")
                    {
                        xml = new ShotsXml();
                        xml.createXML(sfd.FileName, videoName, algorithm, parameter1, parameter2);

                        if (m_ShotInfo != null)
                        {
                            for (int i = 0; i < m_ShotInfo.Length; i++)
                            {
                                xml.addShot(m_ShotInfo[i].shot, m_ShotInfo[i].tags);
                            }
                        }
                    }

                    MessageBox.Show("Save File Successfully!");
                }
            }
            else
            {
                MessageBox.Show("Please load a video file first!");
            }
        }

        private void btAdd_Click(object sender, EventArgs e)
        {
            if (m_ShotInfo == null)
            {
                MessageBox.Show("Please open a file, and execute shot detection!");
            }
            else if (lbPlay.SelectedIndex <= 0)
            {
                MessageBox.Show("Please select one shot to which you want to add tags!");
            }
            else
            {
                if (tbTags.Text != "")
                {
                    /* do not add the tags which already exist */
                    for (int i = 0; i < lbTags.Items.Count; i++)
                    {
                        if (tbTags.Text.Equals(lbTags.Items[i]))
                        {
                            MessageBox.Show("Already have the same tag!");
                            tbTags.Text = "";
                            return;
                        }
                    }

                    lbTags.Items.Add(tbTags.Text);

                    /* use lbPlay's index to connect shot and tags */
                    int index = lbPlay.SelectedIndex - 1;
                    if (m_ShotInfo[index].tags == null)
                        m_ShotInfo[index].tags = new List<string>();
                    m_ShotInfo[index].tags.Add(tbTags.Text);
                    for (int i = 0; i < all_ShotInfo.Length; i++)
                    {
                        if (all_ShotInfo[i].shot.frame1 == lbShots[index].frame1)
                        {
                            all_ShotInfo[i].tags.Add(tbTags.Text);
                            break;
                        }
                    }
                }
            }

            tbTags.Text = "";
        }

        private void btDel_Click(object sender, EventArgs e)
        {
            if (lbTags.SelectedIndex != -1)
            {
                /* remove the tag both listbox and ShotInfo */
                m_ShotInfo[lbPlay.SelectedIndex - 1].tags.RemoveAt(lbTags.SelectedIndex);
                lbTags.Items.RemoveAt(lbTags.SelectedIndex);
            }
        }

        private void bFrame_Click(object sender, EventArgs e)
        {
            if (m_play != null)
            {
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    m_play.bmp.Save(saveFileDialog.FileName.ToString(), System.Drawing.Imaging.ImageFormat.Jpeg);
                }
            }
            else
            {
                MessageBox.Show("First load a video, click on browse and play the file.");
            }
        }

        private void bRetrieveShots_Click(object sender, EventArgs e)
        {
            if (m_play != null)
            {
                if (m_ShotInfo == null)
                {
                    MessageBox.Show("Please open a file, and execute shot detection!");
                }
                else
                {
                    if (tbTags.Text != "")
                    {
                        string word = tbTags.Text;
                        lbShots = new List<Shot>();
                        List<ShotInfo> templist = new List<ShotInfo>();
                        for (int i = 0; i < all_ShotInfo.Length; i++)
                        {
                            if (all_ShotInfo[i].tags.Contains(word, StringComparer.OrdinalIgnoreCase))
                            {
                                templist.Add(all_ShotInfo[i]);
                            }
                        }
                        m_ShotInfo = new ShotInfo[templist.Count];
                        while (lbPlay.Items.Count > 1) lbPlay.Items.RemoveAt(1);
                        for (int i = 0; i < m_ShotInfo.Length; i++)
                        {
                            m_ShotInfo[i] = templist[i];
                            lbShots.Add(templist[i].shot);
                            lbPlay.Items.Add(string.Format("Shot{0}:{1:N}-{2:N}  Frame #{3}-> #{4}", i + 1, templist[i].shot.start, templist[i].shot.end, templist[i].shot.frame1, templist[i].shot.frame2));
                        }
                    }
                    else
                    {
                        lbShots = new List<Shot>();
                        m_ShotInfo = new ShotInfo[all_ShotInfo.Length];
                        while (lbPlay.Items.Count > 1) lbPlay.Items.RemoveAt(1);
                        for (int i = 0; i < m_ShotInfo.Length; i++)
                        {
                            m_ShotInfo[i] = all_ShotInfo[i];
                            lbShots.Add(all_ShotInfo[i].shot);
                            lbPlay.Items.Add(string.Format("Shot{0}:{1:N}-{2:N}  Frame #{3}-> #{4}", i + 1, all_ShotInfo[i].shot.start, all_ShotInfo[i].shot.end, all_ShotInfo[i].shot.frame1, all_ShotInfo[i].shot.frame2));
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Please load a file first!");
            }
        }


    }
}
