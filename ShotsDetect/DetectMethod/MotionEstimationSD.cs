using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class MotionEstimationSD : DetectMethod
{
    private double[] pGreyValue;

    //search window
    private const int BlockSize = 16;

    public MotionEstimationSD(double p1, double p2, int videoHeight, int videoWidth)
    {
        this.m_p1 = p1;
        this.m_p2 = p2;
        this.m_videoHeight = videoHeight;
        this.m_videoWidth = videoWidth;

        pGreyValue = new double[m_videoHeight * m_videoWidth];
    }

    /// <summary>
    /// implement the Motion estimation algorithm
    /// </summary>
    /// <param name="pBuffer"></param>
    /// <returns></returns>
    public override unsafe bool DetectShot(IntPtr pBuffer)
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

        //search each block
        for (int y = 0; y < block_y; y++)
        {
            for (int x = 0; x < block_x; x++)
            {
                //select the search method
                switch (search_method)
                {
                    case 0:
                        break;
                    case 1:
                        sum_sad += search_SBS(x, y, greyValue);
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

    /// <summary>
    /// simple block search method, only search 9 blocks around the current block
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    private unsafe uint search_SBS(int x, int y, double[] p)
    {
        uint sad = 0xffffffff;

        for (int j = -BlockSize; j <= BlockSize; j += BlockSize)
            for (int i = -BlockSize; i <= BlockSize; i += BlockSize)
                SAD(x, y, i, j, &sad, p);

        return sad;
    }

    /// <summary>
    /// diamond search method.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    private unsafe uint search_DS(int x, int y, double[] p)
    {
        uint sad = 0xffffffff;
        //nine points pattern
        int[,] LDS = new int[9, 2] { { 0, 0 }, { 0, 2 }, { -1, 1 }, { -2, 0 }, { -1, -1 }, { 0, -2 }, { 1, -1 }, { 2, 0 }, { 1, 1 } };
        //four points pattern
        int[,] SDS = new int[5, 2] { { 0, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 0 } };

        int mx, my, mvx, mvy;
        mx = my = 0;

        //search the nine position to find the match block
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

        //search the four positon to find the best match block
        for (int i = 0; i < 5; i++)
            SAD(x, y, mvx + SDS[i, 0], mvy + SDS[i, 1], &sad, p);

        return sad;
    }

    //sum the difference of two blocks
    private unsafe int SAD(int x, int y, int dx, int dy, uint* best_sad, double[] p)
    {
        //(ox,oy) is the current block position; (rx,ry) is the reference block position 
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
}