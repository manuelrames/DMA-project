using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;

using DirectShowLib;

namespace ShotsDetect
{
    internal class Capture : IDisposable
    {
        enum GraphState
        {
            Stopped,
            Paused,
            Running,
            Exiting
        }

        #region Member variables

        // File name we are playing
        private string m_sFileName;

        // graph builder interfaces
        private IFilterGraph2 m_FilterGraph;
        private IMediaControl m_mediaCtrl;
        private IMediaEvent m_mediaEvent;
        private IMediaPosition m_mediaPosition;

        // Used to grab current snapshots
        ISampleGrabber m_sampGrabber = null;

        // Event used by Media Event thread
        private ManualResetEvent m_mre;

        // Current state of the graph (can change async)
        volatile private GraphState m_State = GraphState.Stopped;

        #endregion

        // Release everything.
        public void Dispose()
        {
            CloseInterfaces();
        }
        ~Capture()
        {
            CloseInterfaces();
        }

        // Event that is called when a clip finishs playing
        public event CaptureEvent StopPlay;
        public delegate void CaptureEvent(Object sender);

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        public Capture(Control hWin, string FileName)
        {
            try
            {
                int hr;
                IntPtr hEvent;

                m_sFileName = FileName;

                SetupGraph(hWin, FileName);

                // Get the event handle the graph will use to signal
                // when events occur
                hr = m_mediaEvent.GetEventHandle(out hEvent);
                DsError.ThrowExceptionForHR(hr);

                // Wrap the graph event with a ManualResetEvent
                m_mre = new ManualResetEvent(false);
#if USING_NET11
                m_mre.Handle = hEvent;
#else
                m_mre.SafeWaitHandle = new Microsoft.Win32.SafeHandles.SafeWaitHandle(hEvent, true);
#endif

                // Create a new thread to wait for events
                Thread t = new Thread(new ThreadStart(this.EventWait));
                t.Name = "Media Event Thread";
                t.Start();
            }
            catch
            {
                Dispose();
                throw;
            }

        }

        // Wait for events to happen.  This approach uses waiting on an event handle.
        // The nice thing about doing it this way is that you aren't in the windows message
        // loop, and don't have to worry about re-entrency or taking too long.  Plus, being
        // in a class as we are, we don't have access to the message loop.
        // Alternately, you can receive your events as windows messages.  See
        // IMediaEventEx.SetNotifyWindow.
        private void EventWait()
        {
            // Returned when GetEvent is called but there are no events
            const int E_ABORT = unchecked((int)0x80004004);

            int hr;
            IntPtr p1, p2;
            EventCode ec;

            do
            {
                // Wait for an event
                m_mre.WaitOne(-1, true);

                // Avoid contention for m_State
                lock (this)
                {
                    // If we are not shutting down
                    if (m_State != GraphState.Exiting)
                    {
                        // Read the event
                        for (
                            hr = m_mediaEvent.GetEvent(out ec, out p1, out p2, 0);
                            hr >= 0;
                            hr = m_mediaEvent.GetEvent(out ec, out p1, out p2, 0)
                            )
                        {
                            // Write the event name to the debug window
                            Debug.WriteLine(ec.ToString());

                            // If the clip is finished playing
                            if (ec == EventCode.Complete)
                            {
                                // Call Stop() to set state
                                Stop();

                                // Throw event
                                if (StopPlay != null)
                                {
                                    StopPlay(this);
                                }
                            }

                            // Release any resources the message allocated
                            hr = m_mediaEvent.FreeEventParams(ec, p1, p2);
                            DsError.ThrowExceptionForHR(hr);
                        }

                        // If the error that exited the loop wasn't due to running out of events
                        if (hr != E_ABORT)
                        {
                            DsError.ThrowExceptionForHR(hr);
                        }
                    }
                    else
                    {
                        // We are shutting down
                        break;
                    }
                }
            } while (true);
        }

        // Build the capture graph for grabber and renderer.</summary>
        // (Control to show video in, Filename to play)
        private void SetupGraph(Control hWin, string FileName)
        {
            int hr;

            IBaseFilter ibfRenderer = null;
            IPin iPinInFilter = null;
            IPin iPinOutFilter = null;
            IPin iPinInDest = null;

            m_FilterGraph = new FilterGraph() as IFilterGraph2;

            ICaptureGraphBuilder2 icgb2 = new CaptureGraphBuilder2() as ICaptureGraphBuilder2;

            try
            {
                hr = icgb2.SetFiltergraph(m_FilterGraph);
                DsError.ThrowExceptionForHR(hr);

                IBaseFilter sourceFilter = null;
                hr = m_FilterGraph.AddSourceFilter(FileName, "Ds.NET FileFilter", out sourceFilter);
                DsError.ThrowExceptionForHR( hr );

                // Hopefully this will be the video pin
                IPin iPinOutSource = DsFindPin.ByDirection(sourceFilter, PinDirection.Output, 0);

                // Get the SampleGrabber interface
                m_sampGrabber = (ISampleGrabber) new SampleGrabber();
                IBaseFilter baseGrabFlt = (IBaseFilter)	m_sampGrabber;

                // Configure the Sample Grabber
                ConfigureSampleGrabber(m_sampGrabber);

                iPinInFilter = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Input, 0);
                iPinOutFilter = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Output, 0);

                // Add it to the filter
                hr = m_FilterGraph.AddFilter( baseGrabFlt, "Ds.NET Grabber" );
                DsError.ThrowExceptionForHR( hr );

                hr = m_FilterGraph.Connect(iPinOutSource, iPinInFilter);
                DsError.ThrowExceptionForHR(hr);

                // Get the default video renderer
                ibfRenderer = (IBaseFilter)new VideoRendererDefault();

                // Add it to the graph
                hr = m_FilterGraph.AddFilter(ibfRenderer, "Ds.NET VideoRendererDefault");
                DsError.ThrowExceptionForHR(hr);
                iPinInDest = DsFindPin.ByDirection(ibfRenderer, PinDirection.Input, 0);

                // Connect the graph.  Many other filters automatically get added here
                hr = m_FilterGraph.Connect(iPinOutFilter, iPinInDest);
                DsError.ThrowExceptionForHR(hr);

                // Configure the Video Window
                IVideoWindow videoWindow = m_FilterGraph as IVideoWindow;
                ConfigureVideoWindow(videoWindow, hWin);

                // Grab some other interfaces
                m_mediaEvent = m_FilterGraph as IMediaEvent;
                m_mediaCtrl = m_FilterGraph as IMediaControl;
                m_mediaPosition = m_FilterGraph as IMediaPosition;
            }
            finally
            {
                if (icgb2 != null)
                {
                    Marshal.ReleaseComObject(icgb2);
                    icgb2 = null;
                }
            }
        }

        // Configure the video window
        private void ConfigureVideoWindow(IVideoWindow videoWindow, Control hWin)
        {
            int hr;

            // Set the output window
            hr = videoWindow.put_Owner(hWin.Handle);
            DsError.ThrowExceptionForHR(hr);

            // Set the window style
            hr = videoWindow.put_WindowStyle((WindowStyle.Child | WindowStyle.ClipChildren | WindowStyle.ClipSiblings));
            DsError.ThrowExceptionForHR(hr);

            // Make the window visible
            hr = videoWindow.put_Visible(OABool.True);
            DsError.ThrowExceptionForHR(hr);

            // Position the playing location
            Rectangle rc = hWin.ClientRectangle;
            hr = videoWindow.SetWindowPosition(0, 0, rc.Right, rc.Bottom);
            DsError.ThrowExceptionForHR(hr);
        }

        // Set the options on the sample grabber
        private void ConfigureSampleGrabber(ISampleGrabber sampGrabber)
        {
            AMMediaType media;
            int hr;

            // Set the media type to Video/RBG24
            media = new AMMediaType();
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.RGB24;
            media.formatType = FormatType.VideoInfo;
            hr = sampGrabber.SetMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            DsUtils.FreeAMMediaType(media);
            media = null;

            // Configure the samplegrabber
            hr = sampGrabber.SetBufferSamples(true);
            DsError.ThrowExceptionForHR(hr);
        }

        // Return the currently playing file name
        public string FileName
        {
            get
            {
                return m_sFileName;
            }
        }

        public void Start()
        {
            // If we aren't already playing (or shutting down)
            if (m_State == GraphState.Stopped || m_State == GraphState.Paused)
            {
                int hr = m_mediaCtrl.Run();
                DsError.ThrowExceptionForHR(hr);

                m_State = GraphState.Running;
            }
        }

        public void Stop()
        {
            // Can only Stop when playing or paused
            if (m_State == GraphState.Running || m_State == GraphState.Paused)
            {
                int hr = m_mediaCtrl.Stop();
                DsError.ThrowExceptionForHR(hr);

                m_State = GraphState.Stopped;
            }
        }

        public void getTime(out double currentPosition, out double durationTime)
        {
            m_mediaPosition.get_Duration(out durationTime);
            m_mediaPosition.get_CurrentPosition(out currentPosition);
        }

        // Reset the clip back to the beginning
        public void Rewind()
        {
            int hr;

            //IMediaPosition imp = m_FilterGraph as IMediaPosition;
            hr = m_mediaPosition.put_CurrentPosition(0);
        }

        // Shut down capture
        private void CloseInterfaces()
        {
            int hr;

            lock (this)
            {
                if (m_State != GraphState.Exiting)
                {
                    m_State = GraphState.Exiting;

                    // Release the thread (if the thread was started)
                    if (m_mre != null)
                    {
                        m_mre.Set();
                    }
                }

                if (m_mediaCtrl != null)
                {
                    // Stop the graph
                    hr = m_mediaCtrl.Stop();
                    m_mediaCtrl = null;
                }

                if (m_sampGrabber != null)
                {
                    Marshal.ReleaseComObject(m_sampGrabber);
                    m_sampGrabber = null;
                }

                if (m_FilterGraph != null)
                {
                    Marshal.ReleaseComObject(m_FilterGraph);
                    m_FilterGraph = null;
                }
            }
            GC.Collect();
        }
    }
}
