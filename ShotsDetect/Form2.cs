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
        
        ShotsDetect m_detect;

        public Form2()
        {
            InitializeComponent();
        }

        public Form2(string p)
        {
            InitializeComponent();
            m_detect = new ShotsDetect(p);
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
            if (cbAlgorithm.SelectedItem.ToString().Equals("Pixel Difference"))
            {
                algorithm = State.pxl_diff;
                p1.Text = "treshold 1";
                p2.Text = "treshold 2";
                alg_expl.Text = "Calculate pixel-wise differences in video.";
                p1_expl.Text = "Treshold for pixel difference to count.";
                p2_expl.Text = "Treshold for 2 frames to be counted as a shot transition. High values result in less shot transitions to be found";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Motion Estimation"))
            {
                algorithm = State.mot_est;
                p1.Text = "";
                p2.Text = "";
                alg_expl.Text = "";
                p1_expl.Text = "";
                p2_expl.Text = "";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Global Histogram"))
            {
                algorithm = State.gl_hist;
                p1.Text = "";
                p2.Text = "";
                alg_expl.Text = "";
                p1_expl.Text = "";
                p2_expl.Text = "";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Local Histogram"))
            {
                algorithm = State.loc_hist;
                p1.Text = "";
                p2.Text = "";
                alg_expl.Text = "";
                p1_expl.Text = "";
                p2_expl.Text = "";
            }
            else if (cbAlgorithm.SelectedItem.ToString().Equals("Generalized"))
            {
                algorithm = State.gen;
                p1.Text = "";
                p2.Text = "";
                alg_expl.Text = "";
                p1_expl.Text = "";
                p2_expl.Text = "";
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
            m_detect.setAlgorithm((int)algorithm);
            try
            {
                m_detect.setP1(Double.Parse(tbP1.Text));
                m_detect.setP2(Double.Parse(tbP2.Text));

                Cursor.Current = Cursors.WaitCursor;
                frameTime.Enabled = true;
                m_detect.Start();
                m_detect.WaitUntilDone();
                frameTime.Enabled = false;

                // Final update
                tbFrameNum.Text = m_detect.m_count.ToString();
                tbShotsNum.Text = m_detect.m_shots.ToString();

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
            }
        }
    }
}
