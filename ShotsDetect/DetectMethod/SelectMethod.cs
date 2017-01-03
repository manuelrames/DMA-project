using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SelectMethod
{
    public DetectMethod getMethod(int algorithm, double p1, double p2, int videoHeight, int videoWidth)
    {
        DetectMethod method = null;
        switch (algorithm)
        {
            case 0:
                method = new PixelDifferenceSD(p1, p2, videoHeight, videoWidth);
                break;
            case 1:
                method = new MotionEstimationSD(p1, p2, videoHeight, videoWidth);
                break;
            case 2:
                method = new GlobalHistogramSD(p1, p2, videoHeight, videoWidth);
                break;
            case 3:
                method = new LocalHistogramSD(p1, p2, videoHeight, videoWidth);
                break;
            case 4:
                method = new GeneralizedSD(p1, p2, videoHeight, videoWidth);
                break;
        }

        return method;
    }
}