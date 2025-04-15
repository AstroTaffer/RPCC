using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RPCC.Cams;
using RPCC.Focus;
using RPCC.Tasks;

namespace RPCC.Utils
{   
    public static class Logger
    {
        internal static ListBox LogBox;
        public static bool DebugMode = false;

        public static void LogTaskSummary(ObservationTask task, string operation)
        {
            string coords = task.RepointCoords != null && task.RepointCoords.Any()
                ? string.Join(" | ", task.RepointCoords)
                : "—";

            string times = task.RepointTimes != null && task.RepointTimes.Any()
                ? string.Join(" | ", task.RepointTimes.Select(t => t.ToString("yyyy-MM-dd HH:mm:ss")))
                : "—";

            string message =
                $"[DB] Task {task.TaskNumber} {operation}:\n" +
                $"  📡 RA/DEC: {task.RaDec} | RA: {task.Ra}, Dec: {task.Dec}\n" +
                $"  🔭 Object: \"{task.Object}\" ({task.ObjectType})\n" +
                $"  👤 Observer: \"{task.Observer}\" | Status: {task.Status}\n" +
                $"  ⏱ Duration: {task.Duration} sec | Exp: {task.Exp} x {task.AllFrames} (Done: {task.DoneFrames})\n" +
                $"  🕒 Start: {task.TimeStart:yyyy-MM-dd HH:mm:ss} | End: {task.TimeEnd:yyyy-MM-dd HH:mm:ss}\n" +
                $"  🔃 LastExp: {task.TimeLastExp:yyyy-MM-dd HH:mm:ss}\n" +
                $"  🧪 FrameType: {task.FrameType} | Filters: {task.Filters} | Bin: {task.Xbin}x{task.Ybin}\n" +
                $"  📍 Repoint Coords: {coords}\n" +
                $"  ⏳ Repoint Times: {times}";

            AddDebugLogEntry(message);
        }


        
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
            var logsDir = $"{Settings.MainOutFolder}\\LOGS\\RPCC_LOGS";
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
            AddLogEntry($"ERROR WHILE {proc}: {e}");
            AddLogEntry($"ERROR WHILE {proc}: filter {cam.Filter}, SN {cam.SerialNumber}, model {cam.ModelName}, file {cam.FileName}");
        }

        internal static void AddError(string proc, Exception e)
        { 
            AddLogEntry($"ERROR WHILE {proc}: {e}");
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