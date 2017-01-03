using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class PixelDifferenceSD : DetectMethod
{

    private double[] pGreyValue;

    public PixelDifferenceSD(double p1, double p2, int videoHeight, int videoWidth)
    {
        this.m_p1 = p1;
        this.m_p2 = p2;
        this.m_videoHeight = videoHeight;
        this.m_videoWidth = videoWidth;

        pGreyValue = new double[m_videoHeight * m_videoWidth];
    }

    public override unsafe bool DetectShot(IntPtr pBuffer)
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
}