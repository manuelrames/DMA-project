using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LocalHistogramSD : DetectMethod
{
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

    public LocalHistogramSD(double p1, double p2, int videoHeight, int videoWidth)
    {
        this.m_p1 = p1;
        this.m_p2 = p2;
        this.m_videoHeight = videoHeight;
        this.m_videoWidth = videoWidth;

        block1Histogram = new double[16 * 16 * 16];
        block2Histogram = new double[16 * 16 * 16];
        block3Histogram = new double[16 * 16 * 16];
        block4Histogram = new double[16 * 16 * 16];
        block5Histogram = new double[16 * 16 * 16];
        block6Histogram = new double[16 * 16 * 16];
        block7Histogram = new double[16 * 16 * 16];
        block8Histogram = new double[16 * 16 * 16];
        block9Histogram = new double[16 * 16 * 16];
    }

    /// <summary>
    /// method that use the Local Histogram Comparison algorithm to detect if 2 consecutives frames belong to different shots.
    /// </summary>
    /// <param name="pBuffer"></param>
    public override unsafe bool DetectShot(IntPtr pBuffer)
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
            totalSimilarity += similarity / 9;
        }

        // If similarity is above the threshold p1, then the two frames correspond to the same shot so
        // no shot difference is detected, otherwise a new shot is found.
        if (totalSimilarity < threshold1)
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
        double[] blockColorHistogram = new double[(int)Math.Pow((double)numberOfBins, 3.0)];
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
}