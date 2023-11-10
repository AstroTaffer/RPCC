using System;
using RPCC.Focus;

namespace RPCC
{
    public class Test
    {
        public static void TestSex()
        {
            var inputF = "C:\\Users\\User\\RiderProjects\\RPCC\\RPCC\\Sex\\GPX-TF16E-48_r_2023-11-09T17-35-16.fits";
            var outputF = inputF.Replace("fits", "cat");
            GetDataFromFits.Sex(inputF, outputF);
        }
    }
}