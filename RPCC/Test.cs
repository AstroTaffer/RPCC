using System;
using System.IO;
using RPCC.Focus;

namespace RPCC
{
    public class Test
    {
        public static void TestSex()
        {
            var inp = "D:\\RoboPhotData\\Images\\2023-11-09_GPX-TF16E-48\\RAW\\i\\new";
            string[] paths = Directory.GetFiles(inp, "*.fits");
            foreach (var path in paths)
            {
                // var outputF = path.Replace("fits", "cat");
                var buf = new GetDataFromFits(path);
                buf.PrintData();
                // Console.ReadKey();
            }
            
        }
    }
}