using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Collections.Generic;

using DirectShowLib;

namespace ShotsDetect
{
    /// <summary>
    /// A shot is defined by de starting and ending frame
    /// extra info is provided by start and endtime
    /// </summary>
    public struct Shot
    {
        public int frame1;
        public int frame2;
        public double start;
        public double end;
    }
    /// <summary>
    ///  Class to stream a video sequence and perform actions with the samples on it (Shot Detection)
    /// </summary>
    internal class ShotsDetect : ISampleGrabberCB, IDisposable
    {

        public Shot createShot(int f1, int f2, double s, double e)
        {
            Shot sh = new Shot();
            sh.frame1 = f1;
            sh.frame2 = f2;
            sh.start = s;
            sh.end = e;
            return sh;
        }

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
        private double[] histogramFrame = new double[16 * 16 * 16];

        /// <summary> Different block histograms for Grey Local Histogram Comparison algorithm
        double[] block1GreyHistogram = new double[16];
        double[] block2GreyHistogram = new double[16];
        double[] block3GreyHistogram = new double[16];
        double[] block4GreyHistogram = new double[16];
        double[] block5GreyHistogram = new double[16];
        double[] block6GreyHistogram = new double[16];
        double[] block7GreyHistogram = new double[16];
        double[] block8GreyHistogram = new double[16];
        double[] block9GreyHistogram = new double[16];

        /*
        /// <summary> Different block histograms for Color Local Histogram Comparison algorithm
        double[] block1ColorHistogram = new double[16*16*16];
        double[] block2ColorHistogram = new double[16*16*16];
        double[] block3ColorHistogram = new double[16*16*16];
        double[] block4ColorHistogram = new double[16*16*16];
        double[] block5ColorHistogram = new double[16*16*16];
        double[] block6ColorHistogram = new double[16*16*16];
        double[] block7ColorHistogram = new double[16*16*16];
        double[] block8ColorHistogram = new double[16*16*16];
        double[] block9ColorHistogram = new double[16*16*16];
        */
        // TODO: look if we need this
        /// Used(?) to grab current snapshots
        ISampleGrabber m_sampGrabber = null;

        private IntPtr pFrame;
        private double[] pGreyValue;
        public int m_count = 0;
        public int m_shots = 0;

        const int BlockSize = 16;

        /// <summary>
        /// The parameters to use in a shot detection algorithm.
        /// </summary>
        double p1, p2;

        /// <summary>
        /// to memorize the shots
        /// </summary>
        Form2 form2;
        int frame_counter = 0;
        double current_start_shot = 0.0;
        bool just_changed = false;

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
        public ShotsDetect(string FileName,Form2 f)
        {
            this.form2 = f;
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
            pGreyValue = new double[m_videoHeight * m_videoWidth];

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
                    MotionEstimationSD(pBuffer);
                    break;
                case 2:
                    GlobalHistogramSD(pBuffer);
                    break;
                case 3:
                    LocalHistogramSD(pBuffer);
                    break;
                case 4:
                    GeneralizedSD();
                    break;

            }

            if (just_changed && m_count != 0)
            {
                form2.shots.Add(createShot(m_count-frame_counter,m_count,current_start_shot,SampleTime));
                frame_counter = 0;
                current_start_shot = SampleTime;
            }
            just_changed = false;
            m_count++;
            frame_counter++;
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

        private unsafe double getGreyValue(Byte* b)
        {
            double greyValue = 0;
            for (int i = 0; i < 3; i++)
            {
                if (i == 0)
                    greyValue += *b * 0.2126;
                else if (i == 1)
                    greyValue += *b * 0.7152;
                else if (i == 2)
                    greyValue += *b * 0.0722;
                b++;
            }

            return greyValue;
        }

        private unsafe void PixelDifferenceSD(IntPtr pBuffer)
        {

            double threshold1 = p1;
            double threshold2 = p2;
            Byte* b = (byte*)pBuffer;
            int diff = 0;

            for (int y = 0; y < m_videoHeight; y++)
            {
                for (int x = 0; x < m_videoWidth; x++)
                {
                    //calculate each pixel's grey value
                    double greyValue = 0;

                    greyValue = getGreyValue(b);

                    //compare the grey value with previous frame
                    if (Math.Abs(pGreyValue[y * m_videoWidth + x] - greyValue) > threshold1)
                        diff++;
                    //forward the grey value as previous frame grey value
                    pGreyValue[y * m_videoWidth + x] = greyValue;
                    b += 3;
                }
            }

            //shot detected when (the num of different pixels) > (the whole pixels * threshold2).
            if ((double)diff > m_videoHeight * m_videoWidth * threshold2)
            {
                m_shots++;
                this.just_changed = true;
            }
        }

        private unsafe int SAD(int x, int y, int dx, int dy, uint* best_sad, double[] p)
        {
            int ox = x * BlockSize;
            int oy = y * BlockSize;
            int rx = ox + dx;
            int ry = oy + dy;

            if (rx < 0 || ry < 0 || rx + BlockSize > m_videoWidth || ry + BlockSize > m_videoHeight)
                return 0;

            uint sad = 0;
            int index1 = ox + oy * m_videoWidth;
            int index2 = rx + ry * m_videoWidth;

            for (int i = 0; i < BlockSize; i++)
            {
                for (int j = 0; j < BlockSize; j++)
                {
                    sad += (uint)Math.Abs(p[index1++] - pGreyValue[index2++]);
                }
                index1 += m_videoWidth - BlockSize;
                index2 += m_videoWidth - BlockSize;
            }

            if (sad >= *best_sad)
                return 0;
            *best_sad = sad;
            return 1;
        }

        private unsafe void MotionEstimationSD(IntPtr pBuffer)
        {
            double threshold = p1;
            int search_method = (int)p2;
            uint sum_sad = 0;

            int block_y = m_videoHeight / BlockSize;
            int block_x = m_videoWidth / BlockSize;
            double tmp = 0;
            double[] greyValue = new double[m_videoWidth * m_videoHeight];
            Byte* b = (Byte*)pBuffer;

            for (int j = 0; j < m_videoHeight; j++)
            {
                for (int i = 0; i < m_videoWidth; i++)
                {
                    greyValue[i + j * m_videoWidth] = getGreyValue(b);
                    b += 3;
                }
            }

            for (int y = 0; y < block_y; y++)
            {
                for (int x = 0; x < block_x; x++)
                {
                    switch (search_method)
                    {
                        case 0:
                            break;
                        case 1:
                            sum_sad += search_BS(x, y, greyValue);
                            break;
                        case 2:
                            sum_sad += search_DS(x, y, greyValue);
                            break;
                    }
                }
            }

            tmp = sum_sad / (block_x * block_y);
            if (tmp > threshold)
            {
                m_shots++;
                this.just_changed = true;
            }

            pGreyValue = greyValue;
        }

        private unsafe uint search_BS(int x, int y, double[] p)
        {
            uint sad = 0xffffffff;

            for (int j = -BlockSize; j <= BlockSize; j += BlockSize)
            {
                for (int i = -BlockSize; i <= BlockSize; i += BlockSize)
                {
                    SAD(x, y, i, j, &sad, p);
                }
            }

            return sad;
        }

        private unsafe uint search_DS(int x, int y, double[] p)
        {
            uint sad = 0xffffffff;
            int[,] LDS = new int[9, 2] { { 0, 0 }, { 0, 2 }, { -1, 1 }, { -2, 0 }, { -1, -1 }, { 0, -2 }, { 1, -1 }, { 2, 0 }, { 1, 1 } };
            int[,] SDS = new int[5, 2] { { 0, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 0 } };

            int mx, my, mvx, mvy;

            mx = my = 0;

            do
            {
                mvx = mx;
                mvy = my;
                for (int i = 0; i < 9; i++)
                {
                    if (SAD(x, y, mvx + LDS[i, 0], mvy + LDS[i, 1], &sad, p) == 1)
                    {
                        mx = mvx + LDS[i, 0];
                        my = mvy + LDS[i, 1];
                    }
                }

            } while (mx != mvx || my != mvy);

            for (int i = 0; i < 5; i++)
                SAD(x, y, mvx + SDS[i, 0], mvy + SDS[i, 1], &sad, p);

            return sad;
        }

        private unsafe int getBinLevel(double b)
        {
            int binLevel = 0;

            if (0 <= b && b <= 15)
                binLevel = 0;
            else if (b > 15 && b <= 31)
                binLevel = 1;
            else if (b > 31 && b <= 47)
                binLevel = 2;
            else if (b > 47 && b <= 63)
                binLevel = 3;
            else if (b > 63 && b <= 79)
                binLevel = 4;
            else if (b > 79 && b <= 95)
                binLevel = 5;
            else if (b > 95 && b <= 111)
                binLevel = 6;
            else if (b > 111 && b <= 127)
                binLevel = 7;
            else if (b > 127 && b <= 143)
                binLevel = 8;
            else if (b > 143 && b <= 159)
                binLevel = 9;
            else if (b > 159 && b <= 175)
                binLevel = 10;
            else if (b > 175 && b <= 191)
                binLevel = 11;
            else if (b > 191 && b <= 207)
                binLevel = 12;
            else if (b > 207 && b <= 223)
                binLevel = 13;
            else if (b > 223 && b <= 239)
                binLevel = 14;
            else if (b > 239 && b <= 255)
                binLevel = 15;

            return binLevel;
        }
        /// <summary>
        /// method that uses the Bhattacharyya distance to compare two histograms
        /// </summary>
        /// <param name="previousHistogram"></param>
        /// <param name="newHistogram"></param>
        /// <returns></returns>
        private double compareBothHistograms(double[] previousHistogram, double[] newHistogram)
        {
            // Calculate the difference between both histograms
            double[] mixedHistogram = new double[newHistogram.Length];
            for (int i = 0; i < newHistogram.Length; i++)
            {
                mixedHistogram[i] = Math.Sqrt(previousHistogram[i] * newHistogram[i]);
            }

            // Values of Bhattacharyya Coefficient ranges from 0 to 1
            double similarity = 0;
            for (int i = 0; i < mixedHistogram.Length; i++)
            {
                // Calculate the degree of similarity
                similarity += mixedHistogram[i];
            }

            return similarity;
        }

        private unsafe double[] calculateColorHistogram(Byte* b, int numberOfBins)
        {
            double[] colorHistogram = new double[(int)Math.Pow((double)numberOfBins, 3.0)];
            // tr, tg and tg contain de values of each Red, Green and Blue, respectively, of each pixel of the frame at a certain time
            int tr = 0;
            int tg = 0;
            int tb = 0;
            for (int x = 0; x < m_videoHeight; x++)
            {
                for (int y = 0; y < m_videoWidth; y++)
                {
                    // Variable used to store the values obteined in the matrix colorHistogram
                    int count = 0;
                    for (int c = 0; c < 3; c++)
                    {
                        if (c == 0)
                            tr = getBinLevel((double)*b);
                        else if (c == 1)
                            tg = getBinLevel((double)*b);
                        else if (c == 2)
                            tb = getBinLevel((double)*b);
                        b++;
                    }
                    // The following arithmetic operations are used to store a 3D histogram (so 3D matrix) in an unidimensional matrix
                    // with the same number of elements as the 3D matrix
                    count = tr + tg * 16 + tb * 16 * 16;
                    colorHistogram[count]++;
                }
            }

            // Normalize the color histogram by dividing each element between the total amount of elements
            for (int i = 0; i < colorHistogram.Length; i++)
            {
                colorHistogram[i] = colorHistogram[i] / (m_videoHeight * m_videoWidth);
            }

            return colorHistogram;
        }

        /*
        /// <summary>
        /// method that calculates the histogram of a frame once its pixel values are converted to the grey scale
        /// </summary>
        /// <param name="b"></param>
        /// <param name="numberOfBins"></param>
        /// <returns></returns>
        private unsafe double[] calculateGreyHistogram(Byte* b, int numberOfBins)
        {
            double[] greyHistogram = new double[numberOfBins];

            for (int x = 0; x < m_videoHeight; x++)
            {
                for (int y = 0; y < m_videoWidth; y++)
                {
                    // Convert RGB values to grey scale values as follows: Y = 0.2126 R + 0.7152 G + 0.0722 B
                    // This corresponds to human eye sensibility to color. See: https://en.wikipedia.org/wiki/Luma_%28video%29 for more info.
                    double greyValue = 0;
                    for (int c = 0; c < 3; c++)
                    {
                        if (c == 0)
                            greyValue += 0.2126 * *b;
                        else if (c == 1)
                            greyValue += 0.7152 * *b;
                        else if (c == 2)
                            greyValue += 0.0722 * *b;
                        b++;
                    }
                    // Increase the histogram bin element position found
                    greyHistogram[getBinLevel(greyValue)]++;

                    //Reset variable greyValue
                    greyValue = 0;

                }
            }

            // Normalize the color histogram by dividing each element between the total amount of elements
            for (int i = 0; i < greyHistogram.Length; i++)
            {
                greyHistogram[i] = greyHistogram[i] / (m_videoHeight * m_videoWidth);
            }

            return greyHistogram;
        } 
        */

        /// <summary>
        /// method that use the Global Histogram Comparison algorithm to detect if 2 consecutives frames belong to different shots.
        /// </summary>
        /// <param name="pBuffer"></param>
        private unsafe void GlobalHistogramSD(IntPtr pBuffer)
        {

            Byte* b = (byte*)pBuffer;
            Byte* b0 = (byte*)pFrame;
            double threshold1 = p1;
            //double threshold2 = p2;
            int numberOfBins = 16;

            // Calculate the grey histogram
            //double[] histogramFrame = calculateHistogram(b0, numberOfBins);
            //double[] histogramBuffer = calculateGreyHistogram(b, numberOfBins);

            // Calculate the color histogram
            //double[] histogramFrame = calculateColorHistogram(b0, numberOfBins);
            double[] histogramBuffer = calculateColorHistogram(b, numberOfBins);

            // Calculate the difference between histograms using Bhattacharyya distance
            double similarity = 0;
            similarity = compareBothHistograms(histogramFrame, histogramBuffer);

            // If similarity is above the threshold p1, then the two frames correspond to the same shot so
            // no shot difference is detected, otherwise a new shot is found.
            if (similarity < threshold1)
            {
                m_shots++;
                just_changed = true;
            }

            // We store the histogram of the buffered frame into the histogramFrame variable. By doing this we don't need
            // to calculate again both histograms, and so only the next frame's histogram is calculated.
            histogramFrame = histogramBuffer;
        }

        private unsafe double[] calculateBlockGreyHistogram(Byte* b, int numberOfBins, int xBlockSize, int yBlockSize)
        {
            double[] blockGreyHistogram = new double[numberOfBins];

            // We need to divide the frame in 9 different blocks, so the pointer must be changed following the architecture of the buffered image
            int count = 0;
            for (int x = 0; x < xBlockSize; x++)
            {
                for (int y = 0; y < yBlockSize; y++)
                {
                    // As we want squared blocks, when the pointer is at the end of a row of the block the pointer 
                    // has to be changed to the next block row as follows (only valid if the frame is divided in 9 blocks)
                    if (count == xBlockSize)
                    {
                        b += 2 * xBlockSize;
                        count = 0;
                    }

                    // Convert RGB values to grey scale values as follows: Y = 0.2126 R + 0.7152 G + 0.0722 B
                    // This corresponds to human eye sensibility to color. See: https://en.wikipedia.org/wiki/Luma_%28video%29 for more info.
                    double greyValue = 0;
                    for (int c = 0; c < 3; c++)
                    {
                        if (c == 0)
                            greyValue += 0.2126 * *b;
                        else if (c == 1)
                            greyValue += 0.7152 * *b;
                        else if (c == 2)
                            greyValue += 0.0722 * *b;
                        b++;
                    }
                    // Increase the histogram bin element position found
                    blockGreyHistogram[getBinLevel(greyValue)]++;
                    //Reset variable greyValue
                    greyValue = 0;
                    //Increment the counter
                    count++;
                }
            }

            // Normalize the color histogram by dividing each element between the total amount of elements
            for (int i = 0; i < blockGreyHistogram.Length; i++)
            {
                blockGreyHistogram[i] = blockGreyHistogram[i] / (xBlockSize * yBlockSize);
            }

            return blockGreyHistogram;
        }

        /// <summary>
        /// method that use the Local Histogram Comparison algorithm to detect if 2 consecutives frames belong to different shots.
        /// </summary>
        /// <param name="pBuffer"></param>
        private unsafe void LocalHistogramSD(IntPtr pBuffer)
        {
            Byte* b = (byte*)pBuffer;
            Byte* b0 = (byte*)pFrame;
            double threshold1 = p1;
            int numberOfBins = 16;
            int numberOfBlocks = 9;

            // We need to divide the frame into 9 different blocks and then compute the histogram for each of those blocks
            // IMPORTANT: If the height and width are not dividable by 3 we need to implement a way not to lose part of the frame!!!!
            int xBlockSize = m_videoWidth / 3;
            int yBlockSize = m_videoHeight / 3;

            // Variable used to store the total similarity between two frames
            double totalSimilarity = 0;

            // Loop used to change the position of the Byte b which points to the initial pixel of each block
            for (int i = 0; i < numberOfBlocks; i++)
            {
                // No index change needed for the first block, when i == 0, as the pointer is already there
                // When next block is on the right side of the previous block, starting point changes as follows
                if (i == 1 || i == 2 || i == 4 || i == 5 || i == 7 || i == 8)
                {
                    b += xBlockSize * 3;

                }
                // When the next block is on the next row of blocks, starting point changes as follows
                else if (i == 3 || i == 6)
                {
                    b += (3 * xBlockSize * yBlockSize) * 3 - 2 * xBlockSize;
                }

                double[] blockGreyHistogram = calculateBlockGreyHistogram(b, numberOfBins, xBlockSize, yBlockSize);

                // Calculate the difference between histograms using Bhattacharyya distance
                double similarity = 0;
                // We store each block histogram of the buffered frame into the block#GreyHistogram variables. By doing this we don't need
                // to calculate again all the histograms, and so only the next frame's block histograms are calculated.
                switch (i)
                {
                    case 0:
                        similarity = compareBothHistograms(block1GreyHistogram, blockGreyHistogram);
                        block1GreyHistogram = blockGreyHistogram;
                        break;
                    case 1:
                        similarity = compareBothHistograms(block2GreyHistogram, blockGreyHistogram);
                        block2GreyHistogram = blockGreyHistogram;
                        break;
                    case 2:
                        similarity = compareBothHistograms(block3GreyHistogram, blockGreyHistogram);
                        block3GreyHistogram = blockGreyHistogram;
                        break;
                    case 3:
                        similarity = compareBothHistograms(block4GreyHistogram, blockGreyHistogram);
                        block4GreyHistogram = blockGreyHistogram;
                        break;
                    case 4:
                        similarity = compareBothHistograms(block5GreyHistogram, blockGreyHistogram);
                        block5GreyHistogram = blockGreyHistogram;
                        break;
                    case 5:
                        similarity = compareBothHistograms(block6GreyHistogram, blockGreyHistogram);
                        block6GreyHistogram = blockGreyHistogram;
                        break;
                    case 6:
                        similarity = compareBothHistograms(block7GreyHistogram, blockGreyHistogram);
                        block7GreyHistogram = blockGreyHistogram;
                        break;
                    case 7:
                        similarity = compareBothHistograms(block8GreyHistogram, blockGreyHistogram);
                        block8GreyHistogram = blockGreyHistogram;
                        break;
                    case 8:
                        similarity = compareBothHistograms(block9GreyHistogram, blockGreyHistogram);
                        block9GreyHistogram = blockGreyHistogram;
                        break;
                }

                // Increment totalSimilarity with each block histogram similarity weighted with the number of blocks
                totalSimilarity = similarity;

            }

            // If similarity is above the threshold p1, then the two frames correspond to the same shot so
            // no shot difference is detected, otherwise a new shot is found.
            if (totalSimilarity < threshold1)
            {
                m_shots++;
                just_changed = true;
            }
        }

        public void GeneralizedSD()
        {

        }
    }
}
