using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Statistics;

namespace RPCC
{
    public class AnalysisImageForFocus
    {
        private const double _maxEll = 0.25;
        private const int MinStars = 20;

        // переменные кадра
        // private double FWHM; 
        private const double FwhmFocused = 3.0;

        // числа в конфиг
        private const double FwhmGood = 3.2;
        private GetDataFromFITS g;

        public AnalysisImageForFocus(GetDataFromFITS getDataFromFits, double rKron)
        {
            g = getDataFromFits;
            var img = g.GetImage();
            StarsNum = g.GetStarsNum();
            
            
            var oneDimBackgroundArray = new double[img.Length];
            var index = 0;
            for (var i = 0; i < img.GetLength(0); i++)
            for (var j = 0; j < img.GetLength(1); j++)
            {
                oneDimBackgroundArray[index] = img[i, j];
                index++;
            }

            var centroids = g.GetStarsCentroids();

            var sigma = oneDimBackgroundArray.StandardDeviation();
            var background = oneDimBackgroundArray.Median();
            var listBackground = oneDimBackgroundArray
                .Where(t => t < background + 3 * sigma && t > background - 3 * sigma).ToList();

            sigma = listBackground.StandardDeviation();
            background = listBackground.Median();

            if (rKron == 0)
            {
                const int box = 50;
                var listKron = new List<double>();
                foreach (var centroid in centroids)
                {
                    var l = (int) centroid[0] - box;
                    if (l < 0) l = 0;

                    var r = (int) centroid[0] + box;
                    if (r > img.GetLength(0)) r = img.GetLength(0) - 1;

                    var d = (int) centroid[1] - box;
                    if (d < 0) d = 0;
                    var u = (int) centroid[1] + box;
                    if (u > img.GetLength(1)) u = img.GetLength(1) - 1;

                    var listItem = new List<double>();
                    for (var i = l; i < r; i++)
                    for (var j = d; j < u; j++)
                        if (img[i, j] > background + sigma)
                            listItem.Add(Math.Sqrt(Math.Pow(centroid[0] - i, 2) + Math.Pow(centroid[1] - j, 2)));

                    var isoPhot = listItem.Max();
                    var maxCount = 0.0;
                    var momentum = 0.0;
                    for (var i = l; i < r; i++)
                    for (var j = d; j < u; j++)
                    {
                        var radii = Math.Sqrt(Math.Pow(centroid[0] - i, 2) + Math.Pow(centroid[1] - j, 2));
                        if (!(radii <= isoPhot)) continue;
                        momentum += radii * img[i, j];
                        maxCount += img[i, j];
                    }
                    listKron.Add(2*momentum/maxCount);
                }
                RKron = listKron.Median();
            }
            else RKron = rKron;

            var listHfd = new List<double>();
            foreach (var centroid in centroids)
            {
                var l = (int) (centroid[0] - RKron);
                if (l < 0) continue;
                var r = (int) (centroid[0] + RKron);
                if (r > img.GetLength(0)) continue;
                var d = (int) (centroid[1] - RKron);
                if (d < 0) continue;
                var u = (int) (centroid[1] + RKron);
                if (u > img.GetLength(1)) continue;
                var rl = r - l;
                var ud = u - d;
                var sliseOfStar = new double[rl, ud];

                for (var i = l; i < r; i++)
                {
                    for (var j = d; j < u; j++)
                    {
                        sliseOfStar[i-l, j-d] = img[i, j]-background;
                        if (sliseOfStar[i - l, j - d] < 0) sliseOfStar[i - l, j - d] = 0;
                        /*Перед вычислением значения HFD важно вычесть
                         среднее значение фона из каждого пикселя.
                         Если этого не сделать, то суммарное влияние 
                         множества пикселей фона превалирует 
                         над несколькими действительно яркими пикселями, 
                         что приводит к тому, что HFD для всех звёзд будет 
                         примерно одинаковым, не смотря на их различную яркость.*/
                    }
                }
                listHfd.Add(GetHfd(sliseOfStar));
            }

            MeanHfd = listHfd.Mean();
        }

        public double RKron { get; }

        public double Ell { get; }

        public double StarsNum { get; }

        public bool IsGoodImg { get; }

        public double MeanHfd { get; }
        

        // private bool CheckImageQuality()
        // {
        //     if (FWHM > _maxEll)
        //     {
        //         // main.AddLogEntry("Images stretched.");
        //         return false;
        //     }
        //     return true;
        // }

        public bool CheckStarsNum()
        {
            return StarsNum > MinStars;
        }

        private double GetHfd(double[,] starImg)
        {
            var sum = 0d;
            var sumDist = 0d;
            const double epsilon = 1e-6d;

            var centerX = (int) (starImg.GetLength(0) / 2d);
            var centerY = (int) (starImg.GetLength(1) / 2d);

            for (var x = 0; x < starImg.GetLength(0); ++x)
            for (var y = 0; y < starImg.GetLength(1); ++y)
                if (Math.Pow(x - centerX, 2.0) + Math.Pow(y - centerY, 2.0) <= Math.Pow(RKron, 2.0))
                {
                    sum += starImg[x, y];
                    sumDist += starImg[x, y] * Math.Sqrt(Math.Pow(x - centerX, 2.0) + Math.Pow(y - centerY, 2.0));
                }

            // NOTE: Multiplying with 2 is required since actually just the HFR is calculated above
            return sum > epsilon ? 2.0 * sumDist / sum : Math.Sqrt(2d) * RKron;
        }
        
        public bool CheckFocused()
        {
            return g.GetMeanFwhm() < FwhmFocused;
        }
    }
}