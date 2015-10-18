﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;

using DirectShowLib;

namespace ShotsDetect
{
    
    /// <summary>
    ///  Class to stream a video sequence and perform actions with the samples on it (Shot Detection)
    /// </summary>
    internal class ShotsDetect : ISampleGrabberCB, IDisposable
    {
        #region Member variables

        // graph builder interfaces
        private IFilterGraph2 m_FilterGraph = null;
        private IMediaControl m_MediaCtrl = null;
        private IMediaEvent m_MediaEvent = null;
        private IMediaPosition m_MediaPosition = null;

        /// <summary> Dimensions of the image, calculated once in constructor. </summary>
        private int m_videoWidth;
        private int m_videoHeight;
        private int m_stride;

        // TODO: look if we need this
        /// Used(?) to grab current snapshots
        ISampleGrabber m_sampGrabber = null;

        private IntPtr pFrame;
        public int m_count = 0;
        public int m_shots = 0;

        /// <summary>
        /// The parameters to use in a shot detection algorithm.
        /// </summary>
        double p1, p2;

        /// <summary>
        /// Defines the algorithm that will be used in CBBuffer()
        /// 0: pixel difference
        /// 1: motion estimation
        /// 2: global history
        /// 3: local history
        /// 4: generalized method
        /// </summary>
        int algorithm;

        #endregion

        #region API

        /// <summary>
        /// code used from sample at <href> http://directshownet.sourceforge.net/index.html </href>
        /// </summary>
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        private static extern void CopyMemory(IntPtr Destination, IntPtr Source, [MarshalAs(UnmanagedType.U4)] uint Length);

        #endregion

        /// <summary>
        /// code used from sample at <href> http://directshownet.sourceforge.net/index.html </href>
        /// </summary>
        public ShotsDetect(string FileName)
        {
            try
            {
                SetupGraph(FileName);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        /// <summary> 
        /// release everything.
        /// </summary>
        public void Dispose()
        {
            CloseInterfaces();
        }
        // Destructor
        ~ShotsDetect()
        {
            CloseInterfaces();
        }

        /// <summary>
        /// code used from sample at <href> http://directshownet.sourceforge.net/index.html </href>
        /// </summary>
        private void SetupGraph(string FileName)
        {
            int hr;

            ISampleGrabber sampGrabber = null;
            IBaseFilter baseGrabFlt = null;
            IBaseFilter capFilter = null;
            IBaseFilter nullrenderer = null;

            // Get the graphbuilder object
            m_FilterGraph = new FilterGraph() as IFilterGraph2;
            m_MediaCtrl = m_FilterGraph as IMediaControl;
            m_MediaEvent = m_FilterGraph as IMediaEvent;

            IMediaFilter m_mediaFilter = m_FilterGraph as IMediaFilter;

            try
            {
                // Add the video source
                hr = m_FilterGraph.AddSourceFilter(FileName, "Ds.NET FileFilter", out capFilter);
                DsError.ThrowExceptionForHR(hr);

                // Get the SampleGrabber interface
                sampGrabber = new SampleGrabber() as ISampleGrabber;
                baseGrabFlt = sampGrabber as IBaseFilter;

                ConfigureSampleGrabber(sampGrabber);

                // Add the frame grabber to the graph
                hr = m_FilterGraph.AddFilter(baseGrabFlt, "Ds.NET Grabber");
                DsError.ThrowExceptionForHR(hr);

                // ---------------------------------
                // Connect the file filter to the sample grabber

                // Hopefully this will be the video pin, we could check by reading it's mediatype
                IPin iPinOut = DsFindPin.ByDirection(capFilter, PinDirection.Output, 0);

                // Get the input pin from the sample grabber
                IPin iPinIn = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Input, 0);

                hr = m_FilterGraph.Connect(iPinOut, iPinIn);
                DsError.ThrowExceptionForHR(hr);

                // Add the null renderer to the graph
                nullrenderer = new NullRenderer() as IBaseFilter;
                hr = m_FilterGraph.AddFilter(nullrenderer, "Null renderer");
                DsError.ThrowExceptionForHR(hr);

                // ---------------------------------
                // Connect the sample grabber to the null renderer

                iPinOut = DsFindPin.ByDirection(baseGrabFlt, PinDirection.Output, 0);
                iPinIn = DsFindPin.ByDirection(nullrenderer, PinDirection.Input, 0);

                hr = m_FilterGraph.Connect(iPinOut, iPinIn);
                DsError.ThrowExceptionForHR(hr);

                // Turn off the clock.  This causes the frames to be sent
                // thru the graph as fast as possible
                hr = m_mediaFilter.SetSyncSource(null);
                DsError.ThrowExceptionForHR(hr);

                // Read and cache the image sizes
                SaveSizeInfo(sampGrabber);
            }
            finally
            {
                if (capFilter != null)
                {
                    Marshal.ReleaseComObject(capFilter);
                    capFilter = null;
                }
                if (sampGrabber != null)
                {
                    Marshal.ReleaseComObject(sampGrabber);
                    sampGrabber = null;
                }
                if (nullrenderer != null)
                {
                    Marshal.ReleaseComObject(nullrenderer);
                    nullrenderer = null;
                }
            }
        }

        /// <summary>
        /// Read and store the properties 
        /// code used from sample at <href> http://directshownet.sourceforge.net/index.html </href>
        /// </summary>
        private void SaveSizeInfo(ISampleGrabber sampGrabber)
        {
            int hr;

            // Get the media type from the SampleGrabber
            AMMediaType media = new AMMediaType();
            hr = sampGrabber.GetConnectedMediaType(media);
            DsError.ThrowExceptionForHR(hr);

            if ((media.formatType != FormatType.VideoInfo) || (media.formatPtr == IntPtr.Zero))
            {
                throw new NotSupportedException("Unknown Grabber Media Format");
            }

            // Grab the size info
            VideoInfoHeader videoInfoHeader = (VideoInfoHeader)Marshal.PtrToStructure(media.formatPtr, typeof(VideoInfoHeader));
            m_videoWidth = videoInfoHeader.BmiHeader.Width;
            m_videoHeight = videoInfoHeader.BmiHeader.Height;
            m_stride = m_videoWidth * (videoInfoHeader.BmiHeader.BitCount / 8);

            pFrame = Marshal.AllocHGlobal(m_stride * m_videoHeight);

            DsUtils.FreeAMMediaType(media);
            media = null;
        }

        /// <summary> 
        /// Set the options on the sample grabber
        /// code used from sample at <href> http://directshownet.sourceforge.net/index.html </href>
        /// </summary>
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

            // Choose to call BufferCB instead of SampleCB
            hr = sampGrabber.SetCallback(this, 1);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary> sample callback, NOT USED. </summary>
        int ISampleGrabberCB.SampleCB(double SampleTime, IMediaSample pSample)
        {
            //int hr;
            //IntPtr pBuffer;
            //int size;
            //long BufferLen;

            //hr = pSample.GetPointer(out pBuffer);
            //size = pSample.GetSize();
            //BufferLen = pSample.GetActualDataLength();

            Marshal.ReleaseComObject(pSample);
            return 0;
        }

        /// <summary>
        /// buffer callback, COULD BE FROM FOREIGN THREAD. 
        /// In this method an algorithm to shot detection will be used
        /// </summary>
        /// <param name="SampleTime"></param>
        /// <param name="pBuffer"></param>
        /// <param name="BufferLen"></param>
        /// <returns></returns>
        int ISampleGrabberCB.BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen)
        {
            switch (algorithm)
            {
                case 0:
                    PixelDifferenceSD(pBuffer);
                    break;
                case 1:
                    break;
                case 2:
                    break;
                case 3:
                    break;
                case 4:
                    break;

            }

            m_count++;
            CopyMemory(pFrame, pBuffer, (uint)BufferLen);

            return 0;
        }

        /// <summary> Shut down capture </summary>
        private void CloseInterfaces()
        {
            int hr;

            try
            {
                if (m_MediaCtrl != null)
                {
                    // Stop the graph
                    hr = m_MediaCtrl.Stop();
                    m_MediaCtrl = null;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            if (m_FilterGraph != null)
            {
                Marshal.ReleaseComObject(m_FilterGraph);
                m_FilterGraph = null;
            }
            GC.Collect();
        }

        /// <summary> capture the next image </summary>
        public void Start()
        {
            int hr = m_MediaCtrl.Run();
            DsError.ThrowExceptionForHR(hr);
        }

        public void WaitUntilDone()
        {
            int hr;
            EventCode evCode;
            const int E_Abort = unchecked((int)0x80004004);

            do
            {
                System.Windows.Forms.Application.DoEvents();
                hr = this.m_MediaEvent.WaitForCompletion(100, out evCode);
            } while (hr == E_Abort);
            DsError.ThrowExceptionForHR(hr);
        }

        /// <summary>
        /// method that should be called in a frame to set the parameter
        /// </summary>
        /// <param name="p1"></param>
        public void setP1(double p1)
        {
            this.p1 = p1;
        }

        /// <summary>
        /// method that should be called in a frame to set the parameter
        /// </summary>
        /// <param name="p1"></param>
        public void setP2(double p2)
        {
            this.p2 = p2;
        }

        /// <summary>
        /// method that should be called in a frame to set the algorithm to use in BufferCB()
        /// </summary>
        /// <param name="p1"></param>
        public void setAlgorithm(int a)
        {
            this.algorithm = a;
        }

        private unsafe void PixelDifferenceSD(IntPtr pBuffer)
        {
            double threshold1 = p1;
            double threshold2 = p2;
            Byte* b = (byte*)pBuffer;
            Byte* b0 = (byte*)pFrame;
            int diff = 0;

            for (int x = 0; x < m_videoHeight; x++)
            {
                for (int y = 0; y < m_videoWidth; y++)
                {
                    for (int c = 0; c < 3; c++)
                    {
                        if (c == 2)
                            if (Math.Abs(*b0 - *b) > threshold1)
                                diff++;
                        b++;
                        b0++;
                    }
                }
            }

            if ((double)diff > m_videoHeight * m_videoWidth * threshold2)
                m_shots++;
        }

        private unsafe void MotionEstimationSD()
        {
        }

        private unsafe void GlobalHistogramSD()
        {
        }

        private unsafe void LocalHistogramSD()
        {
        }

        public void GeneralizedSD()
        {
        }
    }
}