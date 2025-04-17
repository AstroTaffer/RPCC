#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RPCC.Utils
{
    public class DonutsMetrics
    {
        [JsonPropertyName("focus")]
        public int Focus { get; set; }

        [JsonPropertyName("fwhm")]
        public float Fwhm { get; set; }

        [JsonPropertyName("ell")]
        public float Ell { get; set; }

        [JsonPropertyName("stars")]
        public int Stars { get; set; }

        [JsonPropertyName("bkg")]
        public float Bkg { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public class DonutsShift
    {
        [JsonPropertyName("dx")]
        public float Dx { get; set; }
        [JsonPropertyName("dy")]
        public float Dy { get; set; }
        [JsonPropertyName("dalpha")]
        public float Dalpha { get; set; }
        [JsonPropertyName("ddelta")]
        public float Ddelta { get; set; }
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    public static class DonutsRunner
    {
        private static string RunDonuts(string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python",
                Arguments = $"DONUTS.py {arguments}",
                WorkingDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Guid"),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi);
            proc.WaitForExit(3000);

            string output = proc.StandardOutput.ReadToEnd().Trim();
            string error = proc.StandardError.ReadToEnd().Trim();

            if (!string.IsNullOrWhiteSpace(error))
                Logger.AddDebugLogEntry($"[DONUTS stderr] {error}");
            Logger.AddDebugLogEntry($"[DONUTS stdout] {output}");

            return output;
        }

        public static DonutsMetrics GetImageMetrics(string path)
        {
            try
            {
                var output = RunDonuts($"fwhm \"{path}\"");
                // return JsonSerializer.Deserialize<DonutsMetrics>(output) ?? new DonutsMetrics { Error = "No data" };
                var res = JsonSerializer.Deserialize<DonutsMetrics>(output, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                Logger.AddDebugLogEntry($"[DEBUG] JSON-разбор: F={res.Fwhm}, E={res.Ell}, BKG={res.Bkg}, Stars={res.Stars}, Focus={res.Focus}");

                return res;
            }
            catch (Exception ex)
            {
                return new DonutsMetrics { Error = ex.Message };
            }
        }

        public static DonutsShift GetImageShift(string refPath, string newPath)
        {
            try
            {
                var output = RunDonuts($"don \"{refPath}\" \"{newPath}\"");
                return JsonSerializer.Deserialize<DonutsShift>(output) ?? new DonutsShift { Error = "No data" };
            }
            catch (Exception ex)
            {
                return new DonutsShift { Error = ex.Message };
            }
        }
    }
}