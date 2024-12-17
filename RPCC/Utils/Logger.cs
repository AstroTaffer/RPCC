using System;
using System.IO;
using System.Windows.Forms;
using RPCC.Cams;
using RPCC.Focus;

namespace RPCC.Utils
{   
    public static class Logger
    {
        internal static ListBox LogBox;
        public static bool DebugMode = false;
        public static string LogPath = $"{Settings.MainOutFolder}\\LOGS\\RPCC_LOGS";

        internal static void AddDebugLogEntry(string entry)
        {
            if (DebugMode) 
                AddLogEntry(entry);
        }
        
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

        public static void LogFrameInfo(GetDataFromFits fitsAnalysis, string fil)
        {
            AddLogEntry($"{fil}: Status = {fitsAnalysis.Status}, " +
                                 $"FWHM = {fitsAnalysis.Fwhm}, " +
                                 $"ELL = {fitsAnalysis.Ell}, " +
                                 $"Focus pos = {fitsAnalysis.Focus}, " +
                                 $"Num stars = {fitsAnalysis.StarsNum}, " +
                                 $"Background = {fitsAnalysis.Bkg}");
        }

        internal static void SaveLogs()
        {
            var logsDir = LogPath;
            if (!Directory.Exists(logsDir)) Directory.CreateDirectory(logsDir);
            var logsFileName = $"Logs {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.txt";
            try
            {
                using var sw = new StreamWriter($"{logsDir}\\{logsFileName}");
                foreach (string item in LogBox.Items) sw.WriteLine(item);
            }
            catch (NullReferenceException e)
            {
                AddLogEntry($"Logger warning: {e}");
            }
            
        }

        internal static void AddError(string proc, Exception e, ICameraDevice cam)
        { 
            AddLogEntry($"ERROR WHILE {proc}: {e.Message}");
            AddLogEntry($"ERROR WHILE {proc}: filter {cam.Filter}, SN {cam.SerialNumber}, model {cam.ModelName}, file {cam.FileName}");
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