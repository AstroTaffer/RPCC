using System;
using RPCC.Utils;

namespace RPCC.Cams
{
    internal class GeneralImageStat
    {
        /// <summary>
        ///     Функции расчета общей статистики изображения.
        /// </summary>
        internal int max;

        internal double mean;
        internal int min;
        internal double sd;

        internal GeneralImageStat(ushort[][] image)
        {
            max = int.MinValue;
            min = int.MaxValue;
            mean = 0.0;
            sd = 0.0;

            var sum = 0.0;
            var imageHeight = image.Length;
            var imageWidth = image[0].Length;

            for (var i = 0; i < imageHeight; i++)
            for (var j = 0; j < imageWidth; j++)
            {
                sum += image[i][j];
                if (image[i][j] > max)
                    max = image[i][j];
                if (image[i][j] < min)
                    min = image[i][j];
            }

            mean = sum / (imageHeight * imageWidth);

            sum = 0.0;
            for (var i = 0; i < imageHeight; i++)
            for (var j = 0; j < imageWidth; j++)
                sum += Math.Pow(image[i][j] - mean, 2);
            sd = Math.Sqrt(sum / (imageHeight * imageWidth));
        }
    }

    internal class ProfileImageStat
    {
        internal double background;
        internal double centroidXCoordinate;
        internal double centroidYCoordinate;
        internal double fwhm;
        internal int maxValue;

        /// <summary>
        ///     Функции расчета статистики профиля
        /// </summary>
        internal int maxXCoordinate;

        internal int maxYCoordinate;
        internal double snr;

        internal ProfileImageStat(ushort[][] image, ref Settings settings)
        {
            maxValue = int.MinValue;
            for (var i = 0; i < image.Length; i++)
            for (var j = 0; j < image[i].Length; j++)
                if (image[i][j] > maxValue)
                {
                    maxValue = image[i][j];
                    maxYCoordinate = i;
                    maxXCoordinate = j;
                }

            background = 0.0;
            var backgroundCounter = 0;
            double pixelRadius;
            for (var i = 0; i < image.Length; i++)
            for (var j = 0; j < image[i].Length; j++)
            {
                pixelRadius = Math.Sqrt(Math.Pow(i - maxYCoordinate, 2) + Math.Pow(j - maxXCoordinate, 2));
                if (pixelRadius >= settings.AnnulusInnerRadius && pixelRadius <= settings.AnnulusOuterRadius)
                {
                    background += image[i][j];
                    backgroundCounter++;
                }
            }

            background /= backgroundCounter;

            centroidXCoordinate = 0.0;
            centroidYCoordinate = 0.0;
            var centroidTotalValue = 0.0;
            for (var i = 0; i < image.Length; i++)
            for (var j = 0; j < image[i].Length; j++)
            {
                centroidXCoordinate += (image[i][j] - background) * j;
                centroidYCoordinate += (image[i][j] - background) * i;
                centroidTotalValue += image[i][j] - background;
            }

            centroidXCoordinate /= centroidTotalValue;
            centroidYCoordinate /= centroidTotalValue;

            var starFlux = 0.0;
            var skyFlux = 0.0;
            var starPixelCount = 0;
            var skyPixelCount = 0;
            double pixelFlux;
            for (var i = 0; i < image.Length; i++)
            for (var j = 0; j < image[i].Length; j++)
            {
                pixelFlux = image[i][j] - background;
                pixelRadius = Math.Sqrt(Math.Pow(i - centroidYCoordinate, 2) + Math.Pow(j - centroidXCoordinate, 2));

                if (pixelRadius <= settings.ApertureRadius)
                {
                    starFlux += pixelFlux;
                    starPixelCount++;
                }
                else if (pixelRadius >= settings.AnnulusInnerRadius && pixelRadius <= settings.AnnulusOuterRadius)
                {
                    skyFlux += pixelFlux;
                    skyPixelCount++;
                }
            }

            try
            {
                snr = starFlux / Math.Sqrt(starFlux + starPixelCount / skyPixelCount * skyFlux);
            }
            catch (DivideByZeroException)
            {
                snr = double.NaN;
            }

            starFlux = 0.0;
            var weightFlux = 0.0;
            for (var i = 0; i < image.Length; i++)
            for (var j = 0; j < image[i].Length; j++)
            {
                pixelFlux = image[i][j] - background;
                pixelRadius = Math.Sqrt(Math.Pow(i - centroidYCoordinate, 2) + Math.Pow(j - centroidXCoordinate, 2));

                if (pixelRadius <= settings.AnnulusInnerRadius)
                {
                    starFlux += pixelFlux;
                    weightFlux += Math.Pow(pixelRadius, 2) * pixelFlux;
                }
            }

            try
            {
                fwhm = 2.35 * Math.Sqrt(weightFlux / (2 * starFlux));
            }
            catch (DivideByZeroException)
            {
                fwhm = double.NaN;
            }
        }
    }
}