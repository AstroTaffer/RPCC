using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MathNet.Numerics.Statistics;
using nom.tam.fits;
using RPCC.Utils;


/*
 * фалик чтобы вытащить из фитса центроиды, фвхм, эллиптичность
 */
namespace RPCC.Focus
{
    public class GetDataFromFits
    {
        private const double MaxEll = 0.25;
        private const int MinStars = 20;
        private const double FwhmFocused = 3.0;
        private readonly string _path2Cat;
        private List<List<double>> _sortTable;
        public bool Status { get; }
        public int Focus { get; }
        public double Fwhm { get; }
        private double Ell { get; }
        private double StarsNum { get; }
        private bool Quality { set; get; }

        public GetDataFromFits(string path2Fits)
        {
            var outputFile = path2Fits.Replace("fits", "cat");
            _path2Cat = outputFile;
            var fits = new Fits(path2Fits);
            var hdu = (ImageHDU) fits.GetHDU(0);
            Focus = hdu.Header.GetIntValue("FOCUS");
            try
            { 
                if (!File.Exists(outputFile)) Sex(path2Fits, outputFile);
                GetTable();
                
                StarsNum = GetStarsNum();
                Ell = GetEllipticity();
                Fwhm = GetMedianFwhm();
                Quality = CheckImageQuality();

                var cursor = hdu.Header.GetCursor();
                cursor.Key = "END";
                cursor.Add(new HeaderCard("SEXFWHM", Fwhm, "Sextractor median FWHM"));
                cursor.Add(new HeaderCard("SEXELL", Ell, "Sextractor median ellipticity"));
                cursor.Add(new HeaderCard("SEXNSTAR", StarsNum, "Stars in sextractor catalog"));
                cursor.Add(new HeaderCard("QUALITY", Quality, "Quality control flag"));
                hdu.Rewrite();
                fits.Close();
                Status = true;
            }
            catch (Exception e)
            {
                Status = false;
                Logger.AddLogEntry("Can't do SExtract");
            }
        }
        
        private static void Sex(string inputFile, string outputFile)
        {
            var cwd = Directory.GetCurrentDirectory();
            var sex = cwd + @"\Sex\Extract.exe ";
            var dSex = " -c " + cwd + @"\Sex\default.sex";
            var dPar = " -PARAMETERS_NAME " + cwd + @"\Sex\default.par";
            var dFilt = " -FILTER_NAME " + cwd + @"\Sex\tophat_2.5_3x3.conv";
            var nnw = " -STARNNW_NAME " + cwd + @"\Sex\default.nnw";

            var shell = sex + inputFile + dSex + dPar + dFilt + nnw + " -CATALOG_NAME " + outputFile;
            Console.WriteLine(shell);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/C {shell}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            var process = Process.Start(processStartInfo);
            process?.WaitForExit();
        }

        private void GetTable()
        {
            // Открываем файл с таблицей ASCII
            var reader = new StreamReader(_path2Cat);

            // Создаем список списков
            var table = new List<List<double>>();

            // Читаем файл построчно и добавляем каждую строку в список
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line?[0] == '#') continue;
                var row = line?.Split(' ').Where(t => t.Length >= 1).Select(Convert.ToDouble).ToList();
                table.Add(row);
            }

            // Закрываем файл
            reader.Close();

            //сортируем по потоку
            _sortTable = Transpose(table.OrderByDescending(t => t[3]).ToList());
        }

        private static List<List<T>> Transpose<T>(List<List<T>> matrix)
        {
            var result = new List<List<T>>();

            if (matrix == null || matrix.Count == 0) return result;

            var rows = matrix.Count;
            var cols = matrix[0].Count;

            for (var j = 0; j < cols; j++)
            {
                var newRow = new List<T>();
                for (var i = 0; i < rows; i++) newRow.Add(matrix[i][j]);
                result.Add(newRow);
            }

            return result;
        }

        public bool DelOutputs()
        {
            try
            {
                // Check if file exists with its full path    
                if (!File.Exists(_path2Cat)) return false;
                // If file found, delete it    
                File.Delete(_path2Cat);
                // Console.WriteLine("File deleted.");
                return true;
                // _log.AddLogEntry(@"GetDataFromFITS: File not found: "); 
                // Console.WriteLine("File not found");
            }
            catch (IOException ioExp)
            {
                Logger.AddLogEntry(@"GetDataFromFITS delete outputs error: " + ioExp.Message);
                return false;
            }
        }

        public int GetStarsNum()
        {
            return _sortTable[0].Count;
        }

        public double GetEllipticity()
        {
            return _sortTable[1].Median();
        }

        public double GetMedianFwhm()
        {
            return _sortTable[0].Median();
        }
        
        public bool CheckImageQuality()
        {
            if (Ell > MaxEll)
            {
                // игнорируем
                Logger.AddLogEntry("FOCUS: Images stretched");
                return false;
            }

            if (StarsNum < MinStars)
            {
                //мало звезд на обоих кадрах, игнорируем, скорее всего облако. Но может обе в диком дефокусе!
                Logger.AddLogEntry("FOCUS: Few stars on both images");
                return false;
            }

            Logger.AddLogEntry("FOCUS: Focus image is ok!");
            return true;
        }
        
        public bool CheckFocused()
        {
            return Fwhm < FwhmFocused;
        }
    }
}