using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.Statistics;
using RPCC.Focus;

namespace RPCC
{
    public class Test
    {
        public static void TestSex()
        {
            string input = "-0.1~-0.5";
            char delimiter = '~';

            float[] result = input.Split(delimiter)
                .Select(part => float.TryParse(part, out float floatValue) ? floatValue : 0.0f)
                .ToArray();
            Console.WriteLine(result[0]);
            // var inp = "D:\\RoboPhotData\\Images\\2023-11-09_GPX-TF16E-48\\RAW\\i\\new";
            // string[] paths = Directory.GetFiles(inp, "*.fits");
            // List<double> fwhms = new List<double>();
            // List<double> ells = new List<double>(); 
            // foreach (var path in paths)
            // {
            //     // var outputF = path.Replace("fits", "cat");
            //     var buf = new GetDataFromFits(path, false);
            //     // buf.PrintData();
            //     fwhms.Add(buf.Fwhm);
            //     ells.Add(buf.Ell);
            //     // Console.ReadKey();
            // }
            // Console.WriteLine($"fwhm mean {fwhms.Mean()}");
            // Console.WriteLine($"fwhm med {fwhms.Median()}");
            // Console.WriteLine($"ell mean {ells.Mean()}");
            // Console.WriteLine($"ell med {ells.Median()}");
        }
    }
}