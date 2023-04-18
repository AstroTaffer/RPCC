﻿using System;
using nom.tam.fits;

namespace RPCC.Cams
{
    public class ReadFitsFile
    {
        public string Path { get; }

        public Header Header { get; }

        public ushort[,] Data { get; }

        public ReadFitsFile(string fitsFileName)
        {
            Path = fitsFileName;
            var fitsFile = new Fits(fitsFileName);
            var fitsHdu = (ImageHDU) fitsFile.ReadHDU();
            Header = fitsHdu.Header;
            var fitsDataRaw = (Array[]) fitsHdu.Kernel;
            fitsFile.Close();

            var dataHeight = fitsDataRaw.Length;
            var dataWidth = fitsDataRaw[0].Length;
            Data = new ushort[dataHeight, dataHeight];
            for (var i = 0; i < dataHeight; i++)
            {
                for (var j = 0; j < dataWidth; j++)
                {
                    // HACK: Why is this needed and how does it work?
                    var buff = short.MaxValue + (short) fitsDataRaw[i].GetValue(j) + 1;
                    Data[i, j] = (ushort) buff;
                }
            }
        }
    }
}