using System;

namespace RPCC
{
    internal class GeneralImageStat
    {
        /// <summary>
        ///     Функции расчета общей статистики изображения.
        /// </summary>

        internal int max;
        internal int min;
        internal double mean;
        internal double sd;

        internal GeneralImageStat(ushort[][] image)
        {
            max = int.MinValue;
            min = int.MaxValue;
            mean = 0.0;
            sd = 0.0;

            double sum = 0.0;
            int imageHeight = image.Length;
            int imageWidth = image[0].Length;

            for (int i = 0; i < imageHeight; i++)
            {
                for (int j = 0; j < imageWidth; j++)
                {
                    sum += image[i][j];
                    if (image[i][j] > max)
                        max = image[i][j];
                    if (image[i][j] < min)
                        min = image[i][j];
                }
            }
            mean = sum / (imageHeight * imageWidth);

            sum = 0.0;
            for (int i = 0; i < imageHeight; i++)
            {
                for (int j = 0; j < imageWidth; j++)
                {
                    sum += Math.Pow(image[i][j] - mean, 2);
                }
            }
            sd = Math.Sqrt(sum / (imageHeight * imageWidth));
        }
    }

    internal class ProfileImageStat
    {
        /// <summary>
        ///     Функции расчета статистики профиля
        /// </summary>

        internal int maxXCoordinate;
        internal int maxYCoordinate;
        internal int maxValue;
        internal double background;
        internal double centroidXCoordinate;
        internal double centroidYCoordinate;
        internal double snr;
        internal double fwhm;

        internal ProfileImageStat(ushort[][] image, ref Settings settings)
        {
            // TODO: Number of variables can be reduced as well as number of nested cycles

            maxValue = int.MinValue;
            for (int i = 0; i < image.Length; i++)
                for (int j = 0; j < image[i].Length; j++)
                {
                    if (image[i][j] > maxValue)
                    {
                        maxValue = image[i][j];
                        maxYCoordinate = i;
                        maxXCoordinate = j;
                    }
                }

            background = 0.0;
            int backgroundCounter = 0;
            double pixelRadius;
            for (int i = 0; i < image.Length; i++)
                for (int j = 0; j < image[i].Length; j++)
                {
                    pixelRadius = Math.Sqrt(Math.Pow((i - maxYCoordinate), 2) + Math.Pow(j - maxXCoordinate, 2));
                    if (pixelRadius >= settings.AnnulusInnerRadius && pixelRadius <= settings.AnnulusOuterRadius)
                    {
                        background += image[i][j];
                        backgroundCounter++;
                    }
                }
            background /= backgroundCounter;

            centroidXCoordinate = 0.0;
            centroidYCoordinate = 0.0;
            double centroidTotalValue = 0.0;
            for (int i = 0; i < image.Length; i++)
                for (int j = 0; j < image[i].Length; j++)
                {
                    centroidXCoordinate += (image[i][j] - background) * j;
                    centroidYCoordinate += (image[i][j] - background) * i;
                    centroidTotalValue += image[i][j] - background;
                }
            centroidXCoordinate /= centroidTotalValue;
            centroidYCoordinate /= centroidTotalValue;

            double starFlux = 0.0;
            double skyFlux = 0.0;
            int starPixelCount = 0;
            int skyPixelCount = 0;
            double pixelFlux;
            for (int i = 0; i < image.Length; i++)
                for (int j = 0; j < image[i].Length; j++)
                {
                    pixelFlux = image[i][j] - background;
                    pixelRadius = Math.Sqrt(Math.Pow((i - centroidYCoordinate), 2) + Math.Pow(j - centroidXCoordinate, 2));

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
            double weightFlux = 0.0;
            for (int i = 0; i < image.Length; i++)
                for (int j = 0; j < image[i].Length; j++)
                {
                    pixelFlux = image[i][j] - background;
                    pixelRadius = Math.Sqrt(Math.Pow((i - centroidYCoordinate), 2) + Math.Pow(j - centroidXCoordinate, 2));

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