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
    public partial class Form2 : Form
    {
        // state of the form
        enum State { pxl_diff=0, mot_est=1, gl_hist=2, loc_hist=3, gen=4}

        State algorithm=State.pxl_diff;

        string Filename;
        
        ShotsDetect m_detect;

        Form1 form1;

        int time;

        public List<Shot> shots = new List<Shot>();

        public delegate void UpdateProgressBarDelegate(int progress);
        public UpdateProgressBarDelegate UpdateProgressBar;

        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Form1 f, string p)
        {
            InitializeComponent();
            Filename = p;
            this.form1 = f;
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            // GUI stuff for checkbox
            cbAlgorithm.Items.Add("Pixel Difference");
            cbAlgorithm.Items.Add("Motion Estimation");
            cbAlgorithm.Items.Add("Global Histogram");
            cbAlgorithm.Items.Add("Local Histogram");
            cbAlgorithm.Items.Add("Generalized");
            cbAlgorithm.SelectedIndex = cbAlgorithm.FindStringExact("Pixel Difference");
        }

        // does nothing at the moment
        private void label4_Click(object sender, EventArgs e)
        {

        }

        // bad method name, but I don't know how to change it properly
        /// <summary>
        /// The UI depends of the state and algorithm you want to use
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            /* initailize the textboxes */
            tbP1.Text = "";
            tbP2.Text = "";
            tbFrameNum.Text = "";
            tbShotsNum.Text = "";
            tbTime.Text = "00:00:00";

            if (cbAlgorithm.SelectedItem.ToString().Equals("Pixel Difference"))
            {
                algorithm = State.pxl_diff;
                p1.Text = "threshold 1";
                p2.Text = "threshold 2";
                tbP2.Enabled = true;
                tbP2.Visible = true;
                alg_expl.Text = "Calculate pixel-wise differences in video.";
                p1_expl.Text = "Threshold 1: Treshold for pixel difference to count. (50-100)";
                p2_expl.Text = "Threshold 2: Treshold for 2 frames to be counted as a shot transition.\nHigh values result in less shot transitions to be found. (0.3-0.7)";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Motion Estimation"))
            {
                algorithm = State.mot_est;
                p1.Text = "threshold1";
                p2.Text = "search method";
                tbP2.Enabled = true;
                tbP2.Visible = true;
                alg_expl.Text = "Analyzes the motion between two consecutive frames";
                p1_expl.Text = "Threshold 1: the difference between two frames. (7000-10000)";
                p2_expl.Text = "Search method: 1 for simple block search, 2 for diamond search";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Global Histogram"))
            {
                algorithm = State.gl_hist;
                p1.Text = "Bhatt. Coeff.";
                p2.Text = "1 grey. 2 color";
                //tbP1.Text = "0,7";
                //tbP1.Enabled = false;
                tbP2.Enabled = true;
                tbP2.Visible = true;
                alg_expl.Text = "Calculate frame histogram differences in video.";
                p1_expl.Text = "Bhattacharyya Coefficient for histogram comparison.(0.7-0.9)";
                p2_expl.Text = "Enter 1 for grey histograms or 2 for color histograms.";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Local Histogram"))
            {
                algorithm = State.loc_hist;
                p1.Text = "Bhatt. Coeff.";
                p2.Text = "1 grey. 2 color";
                //tbP1.Text = "";
                //tbP1.Enabled = false;
                tbP2.Enabled = true;
                tbP2.Visible = true;
                alg_expl.Text = "Calculate frame local histogram differences in video.";
                p1_expl.Text = "Bhattacharyya Coefficient for histogram comparison.(0.7-0.9)";
                p2_expl.Text = "Enter 1 for grey histograms or 2 for color histograms.";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Generalized"))
            {
                algorithm = State.gen;
                p1.Text = "Bhatt. Coeff.";
                p2.Text = "search window";
                alg_expl.Text = "Based on the global histogram algorithm.\nUsing search window to do backwards search for detecting dissolves";
                p1_expl.Text = "Bhattacharyya Coefficient for histogram comparison.(>0.9)";
                p2_expl.Text = "search window. (4-6)";
            }
        }

        /// <summary>
        /// Executing the shot detection algorithm
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <bug> can only execute one time at the moment </bug>
        /// <bug> can't do anything else while waiting for the end of the execution 
        ///  (Use a different thread to do the algorithm?) </bug>
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                m_detect = new ShotsDetect(Filename, this);
            }
            catch (Exception exception)
            {
                MessageBox.Show("File name error.\nPlease select a correct file.");
                Dispose();
                return;
            }
            shots.Clear();

            m_detect.setAlgorithm((int)algorithm);
            try
            {
                m_detect.setP1(Double.Parse(tbP1.Text));
                m_detect.setP2(Double.Parse(tbP2.Text));

                Cursor.Current = Cursors.WaitCursor;
                frameTime.Enabled = true;
                button1.Enabled = false;
                bLoad.Enabled = false;
                tbP1.Enabled = false;
                tbP2.Enabled = false;
                time = 0;

                m_detect.Start();
                m_detect.WaitUntilDone();

                frameTime.Enabled = false;
                bLoad.Enabled = true;
                button1.Enabled = true;
                tbP1.Enabled = true;
                tbP2.Enabled = true;

                // Final update
                calTime(time);
                tbFrameNum.Text = m_detect.m_count.ToString();
                tbShotsNum.Text = m_detect.m_shots.ToString();

                //add the last shot
                shots.Add(m_detect.createShot(m_detect.m_count - m_detect.frame_counter, 
                            m_detect.m_count, m_detect.current_start_shot, form1.duration));

                lock (this)
                {
                    m_detect.Dispose();
                    m_detect = null;
                }

                Cursor.Current = Cursors.Default;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Please fill in an integer or a double to execute this Shot Detection");
            }
        }

        private void frameTime_Tick(object sender, EventArgs e)
        {
            if (m_detect != null)
            {
                tbFrameNum.Text = m_detect.m_count.ToString();
                tbShotsNum.Text = m_detect.m_shots.ToString();

                time++;
                calTime(time);
            }
        }

        /// <summary>
        /// This function use to calculate the algorithm's operating time
        /// </summary>
        /// <param name="time">the counter of the timer, every 100ms the counter increase 1</param>
        private void calTime(int time)
        {
            int s = time / 10;
            int h = s / 3600;
            int m = (s - (h * 3600)) / 60;
            s = s - (h * 3600 + m * 60);

            tbTime.Text = string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);
        }

        private void bLoad_Click(object sender, EventArgs e)
        {
            form1.updateLbPlay(this.shots);
            form1.algorithm = (int)algorithm;
            form1.parameter1 = double.Parse(tbP1.Text);
            form1.parameter2 = double.Parse(tbP2.Text);
        }
    }
}
