using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using nom.tam.util;
using RPCC.Cams;
using RPCC.Focus;
using RPCC.Utils;

namespace RPCC.Sex;

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using nom.tam.fits;


public class SextractorRunner
{
    private static readonly string cwd = @"C:\";
    private static readonly string Sex = Path.Combine(cwd, @"Sex\Extract.exe");
    private static readonly string dSex = " -c " + Path.Combine(cwd, @"Sex\pipeline.sex");
    private static readonly string dPar = " -PARAMETERS_NAME " + Path.Combine(cwd, @"Sex\pipeline.par");
    private static readonly string dFilt = " -FILTER_NAME " + Path.Combine(cwd, @"Sex\tophat_2.5_3x3.conv");
    private static readonly string NNW = " -STARNNW_NAME " + Path.Combine(cwd, @"Sex\default.nnw");

    public static (int focus, double fwhm, double ell, int nstars, double bkg) Run(ICameraDevice cam)
    {
        string inputFile = cam.LatestImageFilename;
        
        string outputFile = Path.ChangeExtension(inputFile, ".cat");

        string args = $"\"{inputFile}\"{dSex}{dPar}{dFilt}{NNW} -CATALOG_NAME \"{outputFile}\"";

        Console.WriteLine($@"{Sex} {args}");
        
        // --- запускаем SExtractor ---
        var psi = new ProcessStartInfo
        {
            FileName = Sex,
            Arguments = args,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        using (var proc = Process.Start(psi))
        {
            if (!proc.WaitForExit(5_000))
            {
                try { proc.Kill(); } catch { /* ignore */ }
                Console.WriteLine("Timeout SExtractor");
                return (-1, -1, -1, -1, -1);
            }
        }

        if (!File.Exists(outputFile))
        {
            Console.WriteLine("Error: нет cat-файла");
            return (-1, -1, -1, -1, -1);
        }

        // --- разбор .cat (ваш рабочий код оставлен как есть) ---
        string[] lines = File.ReadAllLines(outputFile);
        // Построить словарь "имя колонки -> индекс (0-based)"
        var nameToIdx = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var hdr in lines.Where(l => l.StartsWith("#")))
        {
            // Матч формата: "#  5 FWHM_IMAGE ..."
            var m = Regex.Match(hdr, @"#\s*(\d+)\s+([A-Za-z0-9_]+)");
            if (m.Success)
            {
                int oneBased = int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                string name = m.Groups[2].Value;
                nameToIdx[name] = oneBased - 1; // в данных индексация с нуля
            }
        }

// Функция для безопасного получения индекса по имени
        int Get(string col)
        {
            if (!nameToIdx.TryGetValue(col, out int idx) || idx < 0)
                throw new InvalidDataException($"Колонка '{col}' не найдена в каталоге SExtractor. " +
                                               $"Проверьте pipeline.par, что она включена.");
            return idx;
        }

        int iFwhm = Get("FWHM_IMAGE");
        int iFlux = Get("FLUX_ISOCOR");
        int iFluxErr = Get("FLUXERR_ISOCOR");
        int iFlags = Get("FLAGS");
        int iEll = Get("ELLIPTICITY");
        int iBkg = Get("BACKGROUND");

// Разбор строк данных
        var rows = lines.Where(l => !l.StartsWith("#"))
            .Select(l => l.Split((char[])null, StringSplitOptions.RemoveEmptyEntries))
            .ToList();

        double ParseD(string s) => double.Parse(s, CultureInfo.InvariantCulture);
        int ParseI(string s) => int.Parse(s, CultureInfo.InvariantCulture);

        var filtered = rows.Where(r =>
        {
            double fwhm = ParseD(r[iFwhm]);
            double flux = ParseD(r[iFlux]);
            double ferr = ParseD(r[iFluxErr]);
            int flags = ParseI(r[iFlags]);
            return fwhm > 1 && (flux / ferr > 10) && (flux / ferr < 1000) && flags == 0;
        }).ToList();

        if (filtered.Count < 10)
        {
            Logger.AddLogEntry($"WARNING: NStars < 10, filter {cam.Filter}");
            // return (0, 0, 0, filtered.Count, 0);
        }
        double medFwhm = Math.Round(filtered.Select(r => double.Parse(r[iFwhm])).Median(), 2);
        double medEll = Math.Round(filtered.Select(r => double.Parse(r[iEll])).Median(), 2);
        double medBkg = Math.Round(filtered.Select(r => double.Parse(r[iBkg])).Median(), 2);
        medFwhm = Math.Round(medFwhm * 0.65 * cam.SettingsCollector.Bin, 2);
        
        // using (var mtx = new Mutex(false, mutexName))
        // {
        //     if (!mtx.WaitOne(TimeSpan.FromSeconds(30)))
        //         throw new TimeoutException("Не удалось получить лок на запись FITS.");
        //
        //     try
        //     {
        //         // // --- читаем FITS-заголовок ---
        //         // Fits f = new Fits(inputFile);
        //         // BasicHDU hdu = f.GetHDU(0); // Get the primary HDU (index 0)
        //         // Header header = hdu.Header;
        //         // focus = header.GetIntValue("FOCUS");
        //         // binning = header.GetIntValue("BINNING");
        //         // f.Close();
        //         
        //         // код из блока выше (чтение HDU -> правка -> запись во временный -> Replace)
        //         
        //         // внутри короткой секции под мьютексом:
        //         string tmp = inputFile + ".tmp";
        //         string bak = inputFile + ".bak";
        //
        //         // ОТКРЫВАЕМ И ДЕРЖИМ ОТКРЫТЫМИ ДО ЗАПИСИ
        //         var bfIn = new BufferedFile(inputFile, FileAccess.Read, FileShare.Read);
        //         var fitsIn = new Fits(bfIn);
        //
        //         // читаем все HDU
        //         var hdus = new List<BasicHDU>();
        //         BasicHDU h;
        //         while ((h = fitsIn.ReadHDU()) != null)
        //             hdus.Add(h);
        //
        //         // правим заголовок первичного HDU
        //         var hdr = hdus[0].Header;
        //         focus = hdr.GetIntValue("FOCUS");
        //         binning = hdr.GetIntValue("BINNING");
        //         medFwhm = Math.Round(medFwhm * 0.65 * binning, 2);
        //         SetOrUpdate(hdr, "FWHM", medFwhm,  "Median FWHM from SExtractor [arcsec]");
        //         SetOrUpdate(hdr, "ELL",  medEll,   "Median ellipticity from SExtractor");
        //         SetOrUpdate(hdr, "BKG",  medBkg,   "Median background from SExtractor [ADU]");
        //         SetOrUpdate(hdr, "NSTAR",filtered.Count, "Number of stars used");
        //         hdr.AddHistory($"SExtractor metrics {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");
        //
        //
        //         var outFits = new Fits();
        //         foreach (var x in hdus) outFits.AddHDU(x);
        //
        //         using (var bfOut = new BufferedFile(tmp, FileAccess.ReadWrite, FileShare.None))
        //         {
        //             outFits.Write(bfOut);
        //             bfOut.Flush();
        //         } // bfOut закрыт
        //
        //         // теперь можно закрыть источник
        //         try { fitsIn.Close(); } catch { }
        //         try { bfIn.Close(); } catch { }
        //
        //     // атомичная замена: оригинал -> .bak, tmp -> оригинал
        //         File.Replace(tmp, inputFile, bak, ignoreMetadataErrors: true);
        //     }
        //     finally
        //     {
        //         try { mtx.ReleaseMutex(); } catch { }
        //     }
        // }
        
        try { File.Delete(outputFile); } catch { /* ignore */ }

        return (SerialFocus.CurrentPosition, medFwhm, medEll, filtered.Count, medBkg);
        
    }
    // Вспомогательные функции
     private static string Sha1Hex(string s)
     {
         using var sha1 = SHA1.Create();
         var bytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(s));
         var sb = new StringBuilder(bytes.Length * 2);
         foreach (var b in bytes) sb.Append(b.ToString("x2"));
         return sb.ToString();
     }
     
     public static void WriteSexMetricsToFits(
         string fitsPath,
         double medFwhm,
         double medEll,
         int nStars,
         double medBkg)
     {
         // читаем все HDU
         var hdus = new List<BasicHDU>();
         using (var bf = new BufferedFile(fitsPath, FileAccess.ReadWrite, FileShare.Read))
         {
             var fits = new Fits(bf);
             BasicHDU hdu;
             while ((hdu = fits.ReadHDU()) != null)
                 hdus.Add(hdu);

             if (hdus.Count == 0)
                 throw new InvalidDataException("FITS без HDU.");

             // правим заголовок первичного HDU
             var hdr = hdus[0].Header;

             SetOrUpdate(hdr, "FWHM", medFwhm, "Median FWHM from SExtractor [arcsec]");
             SetOrUpdate(hdr, "ELL",  medEll,   "Median ellipticity from SExtractor");
             SetOrUpdate(hdr, "BKG",  medBkg,   "Median background from SExtractor [ADU]");
             SetOrUpdate(hdr, "NSTAR",nStars,   "Number of stars used by SExtractor");

             // след для аудита
             hdr.AddHistory($"SExtractor metrics written {DateTime.UtcNow:yyyy-MM-ddTHH:mm:ssZ}");

             // перезапись всего файла
             bf.Seek(0, SeekOrigin.Begin);
             bf.SetLength(0);

             var outFits = new Fits();
             foreach (var h in hdus) outFits.AddHDU(h);
             outFits.Write(bf);
             bf.Flush();
         }
     }

     private static void SetOrUpdate(Header hdr, string key, double value, string comment)
     {
         // double — хранится как TDOUBLE
         if (hdr.ContainsKey(key))
             hdr.FindCard(key).Value = value.ToString(CultureInfo.InvariantCulture);
         else
             hdr.AddValue(key, value, comment);
     }

     private static void SetOrUpdate(Header hdr, string key, int value, string comment)
     {
         if (hdr.ContainsKey(key))
             hdr.FindCard(key).Value = value.ToString(CultureInfo.InvariantCulture);
         else
             hdr.AddValue(key, value, comment);
     }
}

public static class Extensions
{
    public static double Median(this IEnumerable<double> source)
    {
        var sorted = source.OrderBy(n => n).ToArray();
        if (sorted.Length == 0) return double.NaN;
        int mid = sorted.Length / 2;
        if (sorted.Length % 2 == 0)
            return (sorted[mid - 1] + sorted[mid]) / 2.0;
        else
            return sorted[mid];
    }
}
