using RPCC.Utils;
using System;

namespace RPCC.Cams
{
    internal class GeneralImageStat
    {
        /// <summary>
        ///     General FITS image statistics
        /// </summary>

        internal int max;
        internal int min;
        internal double mean;
        internal double stddev;

        internal double dnrStart;
        internal double dnrEnd;
        internal double dnrColorScale;

        internal GeneralImageStat()
        {
            Reset();
        }

        internal void Reset()
        {
            max = int.MinValue;
            min = int.MaxValue;
            mean = 0.0;
            stddev = 0.0;

            dnrStart = 0.0;
            dnrEnd = 0.0;
            dnrColorScale = 0.0;
        }

        internal void Calculate(ushort[][] image)
        {
            var sum = 0.0;
            var height = image.Length;
            var width = image[0].Length;

            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                {
                    sum += image[i][j];
                    if (image[i][j] > max) max = image[i][j];
                    if (image[i][j] < min) min = image[i][j];
                }

            mean = sum / (height * width);

            sum = 0.0;
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    sum += Math.Pow(image[i][j] - mean, 2);
            stddev = Math.Sqrt(sum / (height * width));

            dnrStart = mean - Settings.LowerBrightnessSd * stddev;
            if (dnrStart < min) dnrStart = min;
            dnrEnd = mean + Settings.UpperBrightnessSd * stddev;
            if (dnrStart > max) dnrEnd = min;
            dnrColorScale = 255 / (dnrEnd - dnrStart);

        }
    }
}