using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics.Statistics;
using RPCC.Focus;

namespace RPCC
{
    public class Test
    {
        public static void TestSex()
        {
            var inp = "D:\\RoboPhotData\\Images\\2023-11-09_GPX-TF16E-48\\RAW\\i\\new";
            string[] paths = Directory.GetFiles(inp, "*.fits");
            List<double> fwhms = new List<double>();
            List<double> ells = new List<double>(); 
            foreach (var path in paths)
            {
                // var outputF = path.Replace("fits", "cat");
                var buf = new GetDataFromFits(path, false);
                // buf.PrintData();
                fwhms.Add(buf.Fwhm);
                ells.Add(buf.Ell);
                // Console.ReadKey();
            }
            Console.WriteLine($"fwhm mean {fwhms.Mean()}");
            Console.WriteLine($"fwhm med {fwhms.Median()}");
            Console.WriteLine($"ell mean {ells.Mean()}");
            Console.WriteLine($"ell med {ells.Median()}");
        }
    }
}