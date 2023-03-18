using System;
using nom.tam.fits;

namespace RPCC
{
    public class ReadFitsFile
    {
        private Header header;
        private ushort[,] data;
        private string path;

        public string Path => path;
        public Header Header => header;
        public ushort[,] Data => data;

        public ReadFitsFile(string fitsFileName)
        {
            path = fitsFileName;
            var fitsFile = new Fits(fitsFileName);
            var fitsHdu = (ImageHDU) fitsFile.ReadHDU();
            header = fitsHdu.Header;
            var fitsDataRaw = (Array[]) fitsHdu.Kernel;
            fitsFile.Close();

            var dataHeight = fitsDataRaw.Length;
            var dataWidth = fitsDataRaw[0].Length;
            data = new ushort[dataHeight, dataHeight];
            for (var i = 0; i < dataHeight; i++)
            {
                for (var j = 0; j < dataWidth; j++)
                {
                    // HACK: Why is this needed and how does it work?
                    var buff = short.MaxValue + (short) fitsDataRaw[i].GetValue(j) + 1;
                    data[i, j] = (ushort) buff;
                }
            }
        }
    }
}