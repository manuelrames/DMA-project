using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GlobalHistogramSD : DetectMethod
{
    private double[] histogramFrame;

    public GlobalHistogramSD(double p1, double p2, int videoHeight, int videoWidth)
    {
        this.m_p1 = p1;
        this.m_p2 = p2;
        this.m_videoHeight = videoHeight;
        this.m_videoWidth = videoWidth;

        histogramFrame = new double[16 * 16 * 16];
    }

    public override unsafe bool DetectShot(IntPtr pBuffer)
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
            colorHistogram[i] = colorHistogram[i] / (m_videoHeight * m_videoWidth);

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
}
