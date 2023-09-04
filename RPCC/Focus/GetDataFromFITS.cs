using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MathNet.Numerics.Statistics;
using RPCC.Cams;
using RPCC.Utils;


/*
 * фалик чтобы вытащить из фитса центроиды, фвхм, эллиптичность
 */
namespace RPCC.Focus
{
    public class GetDataFromFits
    {
        // private readonly string _path2Fits;
        private readonly string _path2Cat;
        // private const int NumBestStars = 30;
        private List<List<double>> _sortTable;
        // private readonly Logger _log;
        // private string Fil { get; }
        // private readonly ushort[,] _img;
        public bool Status { get; }
        public int Focus { get; }

        // private readonly Logger _log;
        public GetDataFromFits(string path2Fits, bool s)
        {
            // _log = log;
            Status = s;
            var outputFile = path2Fits.Replace("fit", "cat");
            _path2Cat = outputFile;
            var fits = new ReadFitsFile(path2Fits);
            // _img = fits.Data;
            Focus = fits.Header.GetIntValue("FOCUS");
            // Fil = fits.Header.GetStringValue("FILTER");
            if (!File.Exists(outputFile))
            {
                Sex(path2Fits, outputFile);
            }
            GetTable(); 
        }

        // public string Fil { get; }

        public GetDataFromFits(bool s)
        {
            Status = s;
        }

        private static void Sex(string inputFile, string outputFile)
        {   
            var cwd = Directory.GetCurrentDirectory();
            var sex = cwd + @"\Sex\Extract.exe ";
            var dSex = " -c " + cwd + @"\Sex\default.sex" ;
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

        // public ushort[,] GetImage()
        // {
        //     return _img;
        // }

        // public List<double[]> GetStarsCentroids()
        // {
        //     var centroids = Enumerable.Range(0, NumBestStars)
        //         .Select(i => new[]{ _sortTable[0][i], _sortTable[1][i] })
        //         .ToList();
        //     return centroids;
        // }

        public double GetMedianFwhm()
        {
            return _sortTable[0].Median();
        }
    }
}