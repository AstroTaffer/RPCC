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
    public class GetDataFromFITS
    {
        private string path2Fits;
        private string path2Cat;
        private int numBestStars = 20;
        private List<List<double>> sortTable;

        private bool check;  
        // TODO добавить логгер
        
        public GetDataFromFITS(string path2Fits, bool check)    
        {
            /*check определяет является ли кадр фокусировочным или кадром проверки качества фокусировки*/
            this.path2Fits = path2Fits;
            var outputFile = path2Fits.Replace("fits", "cat");
            path2Cat = outputFile;
            this.check = check;
            Sex(path2Fits, outputFile);
            GetTable(); 
        }
        
        private void Sex(string inputFile, string outputFile)
        {   
            var cwd = Directory.GetCurrentDirectory();
            var sex = cwd + @"\Sex\Extract.exe ";
            var dSex = " -c " + cwd + @"\Sex\default.sex" ;
            var dPar = " -PARAMETERS_NAME " + cwd + (check ? @"\Sex\default_a.par" : @"\Sex\default.par");
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
            process.WaitForExit();

            if (process.ExitCode == 0 && File.Exists(outputFile))
                //todo log
                Console.WriteLine("Ok");
            else
                Console.WriteLine("Error");
        }

        private void GetTable()
        {   
            // Открываем файл с таблицей ASCII
            var reader = new StreamReader(path2Cat);

            // Создаем список списков
            var table = new List<List<double>>();

            // Читаем файл построчно и добавляем каждую строку в список
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                if (line?[0] == '#') continue;
                var row = new List<double>(line?.Split(' ').Where(t => t.Length >= 1).Select(Convert.ToDouble));
                // Console.Write(row[5] + "\n");
                table.Add(row);
            }

            // Закрываем файл
            reader.Close();
            
            //сортируем по потоку
            sortTable = Transpose(new List<List<double>>(table.OrderBy(t => check ? t[2]: t[5])).GetRange(0, numBestStars));
            
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


        public bool delOutputs()
        {
            try    
            {    
                // Check if file exists with its full path    
                if (File.Exists(path2Cat))    
                {    
                    // If file found, delete it    
                    File.Delete(path2Cat);    
                    // Console.WriteLine("File deleted.");
                    return true;
                }    
                else
                {
                    return false;
                    // Console.WriteLine("File not found");
                }    
            }    
            catch (IOException ioExp)    
            {    
                // Console.WriteLine(ioExp.Message);
                return false;
            }   
        }

        public int GetStarsNum()
        {
            return sortTable[0].Count;
        }

        public double[,] GetImage()
        {
            //TODO GET IMG
            return new double[2048,2048];
        }

        public List<double[]> GetStarsCentroids()
        {
            List<double[]> centroids = Enumerable.Range(0, numBestStars)
                .Select(i => new[]{ sortTable[0][i], sortTable[1][i] })
                .ToList();
            
            return centroids;
        }

        public double GetMeanFwhm()
        {
            return check ? 0.0 : sortTable[2].Mean();
        }
    }
}