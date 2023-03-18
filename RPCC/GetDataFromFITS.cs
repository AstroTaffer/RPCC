using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MathNet.Numerics.Statistics;


/*
 * фалик чтобы вытащить из фитса центроиды, фвхм, эллиптичность
 */
namespace RPCC
{
    public class GetDataFromFits
    {
        private readonly string _path2Fits;
        private readonly string _path2Cat;
        private const int NumBestStars = 20;
        private List<List<double>> _sortTable;
        private readonly bool _check;
        private readonly Logger _log;
        public GetDataFromFits(string path2Fits, bool check, Logger logger)    
        {
            /*check определяет является ли кадр фокусировочным или кадром проверки качества фокусировки*/
            _log = logger;
            _path2Fits = path2Fits;
            var outputFile = path2Fits.Replace("fits", "cat");
            _path2Cat = outputFile;
            _check = check;
            Sex(path2Fits, outputFile);
            GetTable(); 
        }
        
        private void Sex(string inputFile, string outputFile)
        {   
            var cwd = Directory.GetCurrentDirectory();
            var sex = cwd + @"\Sex\Extract.exe ";
            var dSex = " -c " + cwd + @"\Sex\default.sex" ;
            var dPar = " -PARAMETERS_NAME " + cwd + (_check ? @"\Sex\default_a.par" : @"\Sex\default.par");
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
            if (process == null) return;
            process.WaitForExit();
            
            if (process.ExitCode != 0 || !File.Exists(outputFile))
                _log.AddLogEntry(@"GetDataFromFITS: Error sextractor in file " + _path2Fits);
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
                var row = new List<double>(line?.Split(' ').Where(t => t.Length >= 1).Select(Convert.ToDouble));
                table.Add(row);
            }

            // Закрываем файл
            reader.Close();
            
            //сортируем по потоку
            _sortTable = Transpose(new List<List<double>>(table
                .OrderBy(t => _check ? t[2]: t[5]))
                .GetRange(0, NumBestStars));
            
            // Console.ReadLine();
            // return sortTable;
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
                if (File.Exists(_path2Cat))    
                {    
                    // If file found, delete it    
                    File.Delete(_path2Cat);    
                    // Console.WriteLine("File deleted.");
                    return true;
                }
                _log.AddLogEntry(@"GetDataFromFITS: File not found: "); 
                return false;
                // Console.WriteLine("File not found");
            }    
            catch (IOException ioExp)    
            {    
                // Console.WriteLine(ioExp.Message);
                _log.AddLogEntry(@"GetDataFromFITS: " + ioExp.Message); 
                return false;
            }   
        }

        public int GetStarsNum()
        {
            return _sortTable[0].Count;
        }

        public ushort[,] GetImage()
        {
            return new ReadFitsFile(_path2Fits).Data;
        }

        public List<double[]> GetStarsCentroids()
        {
            var centroids = Enumerable.Range(0, NumBestStars)
                .Select(i => new[]{ _sortTable[0][i], _sortTable[1][i] })
                .ToList();
            return centroids;
        }

        public double GetMeanFwhm()
        {
            return _check ? 0.0 : _sortTable[2].Mean();
        }
    }
}