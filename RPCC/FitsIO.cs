using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nom.tam.fits;

namespace RPCC
{
    internal class RpccFits
    {
        internal Header header;
        internal ushort[][] data;
        // TODO: To completely remove the stutter, think of a way to async reading a fits file
        
        // Create RpccFits by reading an existing FITS file
        internal RpccFits(string fitsFileName)
        {
            var fitsFile = new Fits(fitsFileName);
            var fitsHdu = (ImageHDU)fitsFile.ReadHDU();
            header = fitsHdu.Header;
            var fitsDataRaw = (Array[])fitsHdu.Kernel;
            fitsFile.Close();

            var dataHeight = fitsDataRaw.Length;
            var dataWidth = fitsDataRaw[0].Length;
            data = new ushort[dataHeight][];
            for (var i = 0; i < dataHeight; i++)
            {
                data[i] = new ushort[dataWidth];
                for (var j = 0; j < dataWidth; j++)
                {
                    // HACK: Why is this needed and how does it work?
                    var buff = short.MaxValue + (short)fitsDataRaw[i].GetValue(j) + 1;
                    data[i][j] = (ushort)buff;
                }
            }
        }
    }
}
