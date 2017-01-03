using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShotsDetect
{
    class DetectMethod
    {
        private int m_method;
        private double m_p1;
        private double m_p2;
        private int m_videoHeight;
        private int m_videoWidth;

        private double[] pGreyValue;
        private double[] histogramFrame;

        /// <summary> Different block histograms for Grey Local Histogram Comparison algorithm
        private double[] block1Histogram;
        private double[] block2Histogram;
        private double[] block3Histogram;
        private double[] block4Histogram;
        private double[] block5Histogram;
        private double[] block6Histogram;
        private double[] block7Histogram;
        private double[] block8Histogram;
        private double[] block9Histogram;

        // Variables used in GeneralizedSD method
        int countGeneralizedSD;
        IntPtr pFrame;
        double[,] redTransition;
        double[,] greenTransition;
        double[,] blueTransition;
        double[] redPrevious;
        double[] greenPrevious;
        double[] bluePrevious;
        double[] redForward;
        double[] greenForward;
        double[] blueForward;
        List<double> mutualInformation;
        List<double> Icumm;
        unsafe Byte*[] pointers;



        private const int BlockSize = 16;

        public unsafe DetectMethod(int method, double p1, double p2, int videoHeight, int videoWidth)
        {
            m_method = method;
            m_p1 = p1;
            m_p2 = p2;
            m_videoHeight = videoHeight;
            m_videoWidth = videoWidth;

            //initialize the variable for each method
            switch (m_method)
            {
                case 0:
                case 1:
                    pGreyValue = new double[m_videoHeight * m_videoWidth];
                    break;
                case 2:
                    histogramFrame = new double[16 * 16 * 16];
                    break;
                case 3:
                    block1Histogram = new double[16*16*16];
                    block2Histogram = new double[16*16*16];
                    block3Histogram = new double[16*16*16];
                    block4Histogram = new double[16*16*16];
                    block5Histogram = new double[16*16*16];
                    block6Histogram = new double[16*16*16];
                    block7Histogram = new double[16*16*16];
                    block8Histogram = new double[16*16*16];
                    block9Histogram = new double[16*16*16];
                    break;
                case 4:
                    countGeneralizedSD = 1;
                    redTransition = new double[256, 256];
                    greenTransition = new double[256, 256];
                    blueTransition = new double[256, 256];
                    redPrevious = new double[256];
                    greenPrevious = new double[256];
                    bluePrevious = new double[256];
                    redForward = new double[256];
                    greenForward = new double[256];
                    blueForward = new double[256];
                    mutualInformation = new List<double>();
                    pointers = new Byte*[30];
                    Icumm = new List<double>();
                    break;
            }
        }

        public bool doDetect(IntPtr pBuffer)
        {
            bool result = false;

            switch (m_method)
            {
                case 0:
                    result = PixelDifferenceSD(pBuffer);
                    break;
                case 1:
                    result = MotionEstimationSD(pBuffer);
                    break;
                case 2:
                    result = GlobalHistogramSD(pBuffer);
                    break;
                case 3:
                    result = LocalHistogramSD(pBuffer);
                    break;
                case 4:
                    result = GeneralizedSD(pBuffer);
                    break;
            }

            return result;
        }

        /// <summary>
        /// use to calculate each pixel's grey value
        /// </summary>
        /// <param name="b">the pointer point to the pixel</param>
        /// <returns>grey value</returns>
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

        /// <summary>
        /// compare the difference between two pixels' grey value 
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <returns></returns>
        private unsafe bool PixelDifferenceSD(IntPtr pBuffer)
        {
            double threshold1 = m_p1;
            double threshold2 = m_p2;
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
                return true;
            else
                return false;
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

        private unsafe bool MotionEstimationSD(IntPtr pBuffer)
        {
            double threshold = m_p1;
            int search_method = (int)m_p2;
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

            pGreyValue = greyValue;
            tmp = sum_sad / (block_x * block_y);
            if (tmp > threshold)
                return true;
            else
                return false;
        }

        private unsafe uint search_BS(int x, int y, double[] p)
        {
            uint sad = 0xffffffff;

            for (int j = -BlockSize; j <= BlockSize; j += BlockSize)
                for (int i = -BlockSize; i <= BlockSize; i += BlockSize)
                    SAD(x, y, i, j, &sad, p);

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
                    double greyValue = getGreyValue(b);
                    b += 3;

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
        

        /// <summary>
        /// method that use the Global Histogram Comparison algorithm to detect if 2 consecutives frames belong to different shots.
        /// </summary>
        /// <param name="pBuffer"></param>
        private unsafe bool GlobalHistogramSD(IntPtr pBuffer)
        {
            Byte* b = (byte*)pBuffer;
            double threshold1 = m_p1;
            // Threshold p2 is used to specify if the user wants to use color or grey histograms to detect the shots. 1 for grey; 2 for color.
            double threshold2 = m_p2;
            int numberOfBins = 16;
            double[] histogramBuffer = new double[numberOfBins];

            if (m_p2 == 1)
            {
                // Calculate the grey histogram
                histogramBuffer = calculateGreyHistogram(b, numberOfBins);
            }
            else if (m_p2 == 2)
            {
                // Calculate the color histogram
                histogramBuffer = calculateColorHistogram(b, numberOfBins);
            }

            // Calculate the difference between histograms using Bhattacharyya distance
            double similarity = 0;
            similarity = compareBothHistograms(histogramFrame, histogramBuffer);

            // We store the histogram of the buffered frame into the histogramFrame variable. By doing this we don't need
            // to calculate again both histograms, and so only the next frame's histogram is calculated.
            histogramFrame = histogramBuffer;

            // If similarity is above the threshold p1, then the two frames correspond to the same shot so
            // no shot difference is detected, otherwise a new shot is found.
            if (similarity < threshold1)
                return true;
            else
                return false;
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

        private unsafe double[] calculateBlockColorHistogram(Byte* b, int numberOfBins, int xBlockSize, int yBlockSize)
        {
            double[] blockColorHistogram = new double[(int)Math.Pow((double)numberOfBins,3.0)];
            // tr, tg and tg contain de values of each Red, Green and Blue, respectively, of each pixel of the frame at a certain time
            int tr = 0;
            int tg = 0;
            int tb = 0;
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

                    // Variable used to store the values obteined in the matrix colorHistogram
                    int count2 = 0;
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
                    count2 = tr + tg * 16 + tb * 16 * 16;
                    blockColorHistogram[count2]++;

                    //Increment the counter
                    count++;
                }
            }

            // Normalize the color histogram by dividing each element between the total amount of elements
            for (int i = 0; i < blockColorHistogram.Length; i++)
            {
                blockColorHistogram[i] = blockColorHistogram[i] / (xBlockSize * yBlockSize);
            }

            return blockColorHistogram;
        }

        /// <summary>
        /// method that use the Local Histogram Comparison algorithm to detect if 2 consecutives frames belong to different shots.
        /// </summary>
        /// <param name="pBuffer"></param>
        private unsafe bool LocalHistogramSD(IntPtr pBuffer)
        {
            Byte* b = (byte*)pBuffer;
            double threshold1 = m_p1;
            // Variable used to decide if the detection will be made using grey or color histograms. 1 for grey and 2 for color.
            double threshold2 = m_p2;
            int numberOfBins = 16;
            int numberOfBlocks = 9;
            double[] blockHistogram = new double[numberOfBins];

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

                if (m_p2 == 1) 
                {
                    blockHistogram = calculateBlockGreyHistogram(b, numberOfBins, xBlockSize, yBlockSize);
                }
                else if (m_p2 == 2)
                {
                    blockHistogram = calculateBlockColorHistogram(b, numberOfBins, xBlockSize, yBlockSize);
                }
           
                // Calculate the difference between histograms using Bhattacharyya distance
                double similarity = 0;
                // We store each block histogram of the buffered frame into the block#GreyHistogram variables. By doing this we don't need
                // to calculate again all the histograms, and so only the next frame's block histograms are calculated.
                switch (i)
                {
                    case 0:
                        similarity = compareBothHistograms(block1Histogram, blockHistogram);
                        block1Histogram = blockHistogram;
                        break;
                    case 1:
                        similarity = compareBothHistograms(block2Histogram, blockHistogram);
                        block2Histogram = blockHistogram;
                        break;
                    case 2:
                        similarity = compareBothHistograms(block3Histogram, blockHistogram);
                        block3Histogram = blockHistogram;
                        break;
                    case 3:
                        similarity = compareBothHistograms(block4Histogram, blockHistogram);
                        block4Histogram = blockHistogram;
                        break;
                    case 4:
                        similarity = compareBothHistograms(block5Histogram, blockHistogram);
                        block5Histogram = blockHistogram;
                        break;
                    case 5:
                        similarity = compareBothHistograms(block6Histogram, blockHistogram);
                        block6Histogram = blockHistogram;
                        break;
                    case 6:
                        similarity = compareBothHistograms(block7Histogram, blockHistogram);
                        block7Histogram = blockHistogram;
                        break;
                    case 7:
                        similarity = compareBothHistograms(block8Histogram, blockHistogram);
                        block8Histogram = blockHistogram;
                        break;
                    case 8:
                        similarity = compareBothHistograms(block9Histogram, blockHistogram);
                        block9Histogram = blockHistogram;
                        break;
                }
                // Increment totalSimilarity with each block histogram similarity weighted with the number of blocks
                totalSimilarity += similarity/9;
            }

            // If similarity is above the threshold p1, then the two frames correspond to the same shot so
            // no shot difference is detected, otherwise a new shot is found.
            if (totalSimilarity < threshold1)
                return true;
            else
                return false;
        }
        /// <summary>
        /// this method uses Mutual Information and Canny Edge Detector to detect different shots
        /// All the auxiliary methods needed for this claculations are below the main method.
        /// For more information, see: http://ieeexplore.ieee.org/stamp/stamp.jsp?tp=&arnumber=4722250
        /// </summary>
        /// <param name="pBuffer"></param>
        /// <returns></returns>
        public unsafe bool GeneralizedSD(IntPtr pBuffer)
        {
            Byte* b = (byte*)pBuffer;
            Byte* b0 = (byte*)pFrame;
            double threshold1 = m_p1;
            double threshold2 = m_p2;
            double mutualInfo = 0;
            int windowSize = 30;

            // if statement created to avoid an error when pBuffer is on the first frame.
            if (countGeneralizedSD == 0)
            {
                pointers[countGeneralizedSD] = b;
                countGeneralizedSD++;
                calculateColorTransition(b, b0, countGeneralizedSD);
                pFrame = pBuffer;
                return true;
            }
            // else if statement used from the second frame until the 15th
            else if (countGeneralizedSD > 0 && countGeneralizedSD < windowSize / 2 - 1)
            {
                // First we need to calculate the 3 matrices that contain how many pixels of each grey value
                // changes to another certain grey value, for all the grey values of the frame.
                calculateColorTransition(b, b0, countGeneralizedSD);

                // Calculate the mutual information value between two frames
                mutualInfo = calculateMutualInformation(redTransition, greenTransition, blueTransition);
                mutualInformation.Add(mutualInfo);

                // Store the colorForward matrices for the next grabbed frame
                redPrevious = redForward;
                greenPrevious = greenForward;
                bluePrevious = blueForward;

                // Store the pointer
                pointers[countGeneralizedSD] = b;
                countGeneralizedSD++;

                if (countGeneralizedSD >= 15 && countGeneralizedSD < pointers.Length)
                {
                    // For loop used to store pointers for futher calculation of Icumm
                    for (int i = 15; i < pointers.Length; i++)
                    {
                        b += m_videoHeight * m_videoWidth;
                        pointers[i] = b;
                    }
                }
            }
            // Procedure from the 15th frame, when we can start computing Icumm apart from Mutual Information
            else if (countGeneralizedSD >= windowSize)
            {
                
            }

            return true;
        }

        public unsafe void calculateColorTransition(Byte* b, Byte* b0, int countGeneralizedSD)
        {
            // In case is the first frame, we only need to calculate the colorForward matrices
            // to store them after as colorPrevious matrices and then be used from the second frame
            if (countGeneralizedSD == 0)
            {
                for (int y = 0; y < m_videoHeight; y++)
                {
                    for (int x = 0; x < m_videoWidth; x++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (c == 0)
                            {
                                redForward[*b] += 1 / (m_videoHeight * m_videoWidth);
                            }
                            else if (c == 1)
                            {
                                greenForward[*b] += 1 / (m_videoHeight * m_videoWidth);
                            }
                            else if (c == 2)
                            {
                                blueForward[*b] += 1 / (m_videoHeight * m_videoWidth);
                            }
                            b++;
                        }
                    }
                }
                return;
            }
            else
            {
                // We first reset the matrices to 0
                Array.Clear(redTransition, 0, 256 * 256);
                Array.Clear(greenTransition, 0, 256 * 256);
                Array.Clear(blueTransition, 0, 256 * 256);
                Array.Clear(redForward, 0, 256);
                Array.Clear(greenForward, 0, 256);
                Array.Clear(blueForward, 0, 256);

                // To make the calculation more efficient, we normalize the matrices as wemake the calculations
                for (int y = 0; y < m_videoHeight; y++)
                {
                    for (int x = 0; x < m_videoWidth; x++)
                    {
                        for (int c = 0; c < 3; c++)
                        {
                            if (c == 0)
                            {
                                redTransition[*b, *b0] += 1 / (m_videoHeight * m_videoWidth);
                                redForward[*b] += 1 / (m_videoHeight * m_videoWidth);
                            }
                            else if (c == 1)
                            {
                                greenTransition[*b, *b0] += 1 / (m_videoHeight * m_videoWidth);
                                greenForward[*b] += 1 / (m_videoHeight * m_videoWidth);
                            }
                            else if (c == 2)
                            {
                                blueTransition[*b, *b0] += 1 / (m_videoHeight * m_videoWidth);
                                blueForward[*b] += 1 / (m_videoHeight * m_videoWidth);
                            }
                            b++;
                            b0++;
                        }
                    }
                }
            }
        }

        public double calculateMutualInformation(double[,] redTransition, double[,] greenTransition, double[,] blueTransition)
        {
            double mutualInfo = 0;
            double redMutualInfo = 0;
            double greenMutualInfo = 0;
            double blueMutualInfo = 0;

            for (int i = 0; i < redTransition.GetLength(0); i++)
            {
                for (int j = 0; j < redTransition.GetLength(1); j++)
                {
                    redMutualInfo += redTransition[i, j] * Math.Log10(redTransition[i, j] / (redPrevious[i] * redForward[j]));
                    greenMutualInfo += greenTransition[i, j] * Math.Log10(greenTransition[i, j] / (greenPrevious[i] * greenForward[j]));
                    blueMutualInfo += blueTransition[i, j] * Math.Log10(blueTransition[i, j] / (bluePrevious[i] * blueForward[j]));
                }
            }

            mutualInfo = redMutualInfo + greenMutualInfo + blueMutualInfo;

            return mutualInfo;
        }

        public double calculateJointEntropy(double[,] redTransition, double[,] greenTransition, double[,] blueTransition)
        {
            double jointEnt = 0;
            double redJointEnt = 0;
            double greenJointEnt = 0;
            double blueJointEnt = 0;

            for (int i = 0; i < redTransition.GetLength(0); i++)
            {
                for (int j = 0; j < redTransition.GetLength(1); j++)
                {
                    redJointEnt += redTransition[i, j] * Math.Log10(redTransition[i, j]);
                    greenJointEnt += greenTransition[i, j] * Math.Log10(greenTransition[i, j]);
                    blueJointEnt += blueTransition[i, j] * Math.Log10(blueTransition[i, j]);
                }
            }

            jointEnt = redJointEnt + greenJointEnt + blueJointEnt;

            return jointEnt;
        }
    }
}
