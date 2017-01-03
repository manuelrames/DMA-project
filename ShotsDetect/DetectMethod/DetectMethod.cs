using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public abstract class DetectMethod
{

    protected double m_p1;
    protected double m_p2;
    protected int m_videoHeight;
    protected int m_videoWidth;

    /// <summary>
    /// use to calculate each pixel's grey value
    /// </summary>
    /// <param name="b">the pointer point to the pixel</param>
    /// <returns>grey value</returns>
    public unsafe double getGreyValue(Byte* b)
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

    public unsafe int getBinLevel(double b)
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
    public double compareBothHistograms(double[] previousHistogram, double[] newHistogram)
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

    public abstract bool DetectShot(IntPtr pBuffer);
}