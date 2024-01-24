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
        private const double MaxEll = 0.7;
        private const int MinStars = 20;
        private const double FwhmFocused = 3.0;
        private const double MinLimitFwhm = 1.0;
        private const double MaxLimitFwhm = 20.0;
        private readonly string _path2Cat;
        private List<List<double>> _sortTable;
        public bool Status { get; }
        public int Focus { get; }
        public double Fwhm { get; }
        public double Ell { get; }
        private double StarsNum { get; }
        private bool Quality { set; get; }

        public GetDataFromFits(string path2Fits, bool updateFits = true)
        {
            if (string.IsNullOrEmpty(path2Fits)) return;

            var outputFile = path2Fits.Replace("fits", "cat");
            _path2Cat = outputFile;

            try
            {
                if (!File.Exists(outputFile))
                {
                    FileStream fs = File.Create(outputFile);
                    fs.Close();
                    Sex(path2Fits, outputFile);
                }
                
                GetTable();
                StarsNum = GetStarsNum();
                Ell = GetEllipticity();
                Fwhm = GetMedianFwhm();
                Quality = CheckImageQuality();

                if (updateFits)
                {
                    var fits = new Fits(path2Fits);
                    var hdu = (ImageHDU) fits.GetHDU(0);
                    Focus = hdu.Header.GetIntValue("FOCUS");
                    var cursor = hdu.Header.GetCursor();
                    cursor.Key = "END";
                    cursor.Add(new HeaderCard("SEXFWHM", Fwhm, "Sextractor median FWHM"));
                    cursor.Add(new HeaderCard("SEXELL", Ell, "Sextractor median ellipticity"));
                    cursor.Add(new HeaderCard("SEXNSTAR", StarsNum, "Stars in sextractor catalog"));
                    cursor.Add(new HeaderCard("QUALITY", Quality, "Quality control flag"));
                    fits.Close();
                }

                Status = true;
                DelOutputs();
            }
            catch (Exception e)
            {
                Status = false;
                Console.WriteLine(e);
                Logger.AddLogEntry("Can't do SExtract");
            }
        }
        
        public static void Sex(string inputFile, string outputFile)
        {
            
            var cwd = Directory.GetCurrentDirectory();
            var sex = cwd + @"\Sex\Extract.exe ";
            var dSex = " -c " + cwd + @"\Sex\proc.sex";
            var dPar = " -PARAMETERS_NAME " + cwd + @"\Sex\proc.par";
            var dFilt = " -FILTER_NAME " + cwd + @"\Sex\tophat_2.5_3x3.conv" ;
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
                if (row?[3] > MaxLimitFwhm | row?[3] < MinLimitFwhm)
                {
                    continue;
                }
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
            return _sortTable[4].Median();
        }

        public double GetMedianFwhm()
        {
            return _sortTable[3].Median();
        }
        
        public bool CheckImageQuality()
        {
            if (Ell > MaxEll)
            {
                // игнорируем
                Logger.AddLogEntry($"FOCUS: Images stretched, ell = {Ell}");
                return false;
            }

            if (StarsNum < MinStars)
            {
                //мало звезд на обоих кадрах, игнорируем, скорее всего облако. Но может обе в диком дефокусе!
                Logger.AddLogEntry($"FOCUS: Few stars, num = {StarsNum}");
                return false;
            }

            Logger.AddLogEntry("FOCUS: Focus image is ok!");
            return true;
        }
        
        public bool CheckFocused()
        {
            if (CameraFocus.DeFocus != 0)
            {
                return Fwhm < FwhmFocused + 1;
            }

            if (Fwhm < FwhmFocused)
            {
                Logger.AddLogEntry($"FOCUS: image is focused, fwhm = {Fwhm}");
            }
            else
            {
                Logger.AddLogEntry($"FOCUS: image is not focused, fwhm = {Fwhm}");
            }
            
            return Fwhm < FwhmFocused;
        }

        public void PrintData()
        {
            Console.WriteLine($"stars num {StarsNum}");
            Console.WriteLine($"Ell {Ell}");
            Console.WriteLine($"FWHM {Fwhm}");
            Console.WriteLine($"quality {Quality}");
            Console.WriteLine($"focused {CheckFocused()}");
        }
    }
}