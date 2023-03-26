using System;
using nom.tam.fits;

namespace RPCC
{
    internal class RpccFits
    {
        internal ushort[][] data;

        internal Header header;
        // TODO: To completely remove the stutter, think of a way to async reading a fits file

        // Create RpccFits by reading an existing FITS file
        internal RpccFits(string fitsFileName)
        {
            var fitsFile = new Fits(fitsFileName);
            var fitsHdu = (ImageHDU) fitsFile.ReadHDU();
            header = fitsHdu.Header;
            var fitsDataRaw = (Array[]) fitsHdu.Kernel;
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
                    var buff = short.MaxValue + (short) fitsDataRaw[i].GetValue(j) + 1;
                    data[i][j] = (ushort) buff;
                }
            }
        }

        // Create new empty RpccFits
        internal RpccFits()
        {

        }
    }
}