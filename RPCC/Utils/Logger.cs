using System;
using System.IO;
using System.Windows.Forms;

namespace RPCC.Utils
{
    public static class Logger
    {
        internal static ListBox logBox;

        // internal Logger(ListBox listBox)
        // {
        //     LogBox= listBox;
        // }

        internal static void AddLogEntry(string entry)
        {
            if (logBox.Items.Count >= 1024)
            {
                SaveLogs();
                ClearLogs();
                logBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been saved and cleaned");
            }

            try
            {
                logBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
            }
            catch
            {
                logBox.Invoke((MethodInvoker) delegate
                {
                    logBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
                });
            }
            
        }

        internal static void SaveLogs()
        {
            var logsFilePath = $"Logs {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.txt";
            var sw = new StreamWriter(logsFilePath);
            foreach (string item in logBox.Items) sw.WriteLine(item);
            sw.Close();
        }

        internal static void ClearLogs()
        {
            logBox.Items.Clear();
        }

        internal static void CopyLogItem()
        {
            if (logBox.SelectedItems.Count > 0) Clipboard.SetText(logBox.SelectedItem.ToString());
        }
    }
}