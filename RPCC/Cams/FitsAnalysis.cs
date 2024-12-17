using System;
using RPCC.Utils;

namespace RPCC.Cams;

internal class GeneralImageStat
{
    internal double DnrColorScale;
    private double _dnrEnd;

    internal double DnrStart;

    /// <summary>
    ///     General FITS image statistics
    /// </summary>
    private int _max;

    private double _mean;
    private int _min;
    private double _stddev;

    internal GeneralImageStat()
    {
        Reset();
    }

    internal void Reset()
    {
        _max = int.MinValue;
        _min = int.MaxValue;
        _mean = 0.0;
        _stddev = 0.0;

        DnrStart = 0.0;
        _dnrEnd = 0.0;
        DnrColorScale = 0.0;
    }

    internal void Calculate(ushort[,] image)
    {
        var sum = 0.0;
        var height = image.GetLength(0);
        var width = image.GetLength(1);

        for (var i = 0; i < height; i++)
        for (var j = 0; j < width; j++)
        {
            sum += image[i, j];
            if (image[i, j] > _max) _max = image[i, j];
            if (image[i, j] < _min) _min = image[i, j];
        }

        _mean = sum / (height * width);

        sum = 0.0;
        for (var i = 0; i < height; i++)
        for (var j = 0; j < width; j++)
            sum += Math.Pow(image[i, j] - _mean, 2);
        _stddev = Math.Sqrt(sum / (height * width));

        DnrStart = _mean - Settings.LowerBrightnessSd * _stddev;
        if (DnrStart < _min) DnrStart = _min;
        _dnrEnd = _mean + Settings.UpperBrightnessSd * _stddev;
        if (DnrStart > _max) _dnrEnd = _min;
        DnrColorScale = 255 / (_dnrEnd - DnrStart);
    }
}