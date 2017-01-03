using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class GeneralizedSD : DetectMethod
{
    private double[] histogramFrame;
    private object[] key_frame;

    private int key_counter;
    private int counter;
    private int begain_index;
    private int end_index;
    private int key_num;

    public GeneralizedSD(double p1, double p2, int videoHeight, int videoWidth)
    {
        this.m_p1 = p1;
        this.m_p2 = p2;
        this.m_videoHeight = videoHeight;
        this.m_videoWidth = videoWidth;

        histogramFrame = new double[16];
        key_num = (int)p2 + 1;
        key_frame = new object[key_num];
        for (int i = 0; i < key_frame.Count(); i++)
            key_frame[i] = new double[16];

        counter = 0;
        key_counter = 10;
        begain_index = 0;
        end_index = 0;
    }

    public override unsafe bool DetectShot(IntPtr pBuffer)
    {
        Byte* b = (byte*)pBuffer;
        double threshold1 = m_p1;
        double threshold2 = m_p2;
        int shot_num = 10;
        int frame_num = 10;

        int numberOfBins = 16;
        double[] histogramBuffer = new double[numberOfBins];
        double[] tempBuffer = new double[numberOfBins];
        double similarity = 0;

        // Calculate the grey histogram
        histogramBuffer = calculateGreyHistogram(b, numberOfBins);

        // Calculate the difference between histograms using Bhattacharyya distance     
        similarity = compareBothHistograms(histogramFrame, histogramBuffer);

        // We store the histogram of the buffered frame into the histogramFrame variable. By doing this we don't need
        // to calculate again both histograms, and so only the next frame's histogram is calculated.
        histogramFrame = histogramBuffer;

        //counter for fades
        counter++;

        //compare the previous frame
        // If similarity is above the threshold p1, then the two frames correspond to the same shot so
        // no shot difference is detected, otherwise a new shot is found.
        if (similarity < threshold1 && counter > shot_num)
        {
            counter = 0;
            key_counter = 0;
            begain_index = end_index;
            return true;
        }
        //compare with the previous key frames
        else
        {
            //queue empty
            if (begain_index == end_index)
            {
                end_index = (end_index + 1) % key_num;
                key_frame[end_index] = histogramBuffer;
                key_counter++;
                return false;
            }
            else
            {
                //search the queue
                for (int i = end_index; i != begain_index;)
                {
                    tempBuffer = (double[])key_frame[i];
                    similarity = compareBothHistograms(tempBuffer, histogramBuffer);
                    if (similarity < threshold1 && counter > shot_num)
                    {
                        counter = 0;
                        key_counter = 0;
                        begain_index = end_index;
                        return true;
                    }

                    i = i - 1;
                    if (i < 0)
                        i = key_num - 1;
                }

                //add the key frame
                if (key_counter >= frame_num)
                {
                    //queue full
                    if ((end_index + 1) % key_num == begain_index)
                    {
                        begain_index = (begain_index + 1) % key_num;
                        end_index = (end_index + 1) % key_num;
                        key_frame[end_index] = histogramBuffer;
                    }
                    else 
                    {
                        end_index = (end_index + 1) % key_num;
                        key_frame[end_index] = histogramBuffer;
                    }

                    key_counter = 0;
                }
                key_counter++;

                return false;
            }
        }
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