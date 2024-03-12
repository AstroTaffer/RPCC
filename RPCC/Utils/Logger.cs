using System;
using System.IO;
using System.Windows.Forms;
using RPCC.Focus;

namespace RPCC.Utils
{
    public static class Logger
    {
        internal static ListBox LogBox;
        
        internal static void AddLogEntry(string entry)
        {
            if (LogBox.Items.Count >= 1024)
            {
                SaveLogs();
                ClearLogs();
                LogBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been saved and cleaned");
            }

            try
            {
                LogBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
            }
            catch
            {
                LogBox.Invoke((MethodInvoker) delegate { LogBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}"); });
            }
        }

        public static void LogFrameInfo(GetDataFromFits fitsAnalysis)
        {
            AddLogEntry($"Status = {fitsAnalysis.Status}, " +
                           $"FWHM = {fitsAnalysis.Fwhm}, " +
                           $"ELL = {fitsAnalysis.Ell}, " +
                           $"Focus pos = {fitsAnalysis.Focus}, " +
                           $"Num stars = {fitsAnalysis.StarsNum}," +
                           $"Background = {fitsAnalysis.Bkg}");
        }

        internal static void SaveLogs()
        {
            var logsDir = $"{Settings.MainOutFolder}\\LOGS";
            if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
            var logsFileName = $"Logs {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.txt";
            var sw = new StreamWriter($"{logsDir}\\{logsFileName}");
            foreach (string item in LogBox.Items) sw.WriteLine(item);
            sw.Close();
        }

        internal static void ClearLogs()
        {
            LogBox.Items.Clear();
        }

        internal static void CopyLogItem()
        {
            if (LogBox.SelectedItems.Count > 0) Clipboard.SetText(LogBox.SelectedItem.ToString());
        }
    }
}