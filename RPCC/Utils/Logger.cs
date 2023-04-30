using System;
using System.IO;
using System.Windows.Forms;

namespace RPCC.Utils
{
    public class Logger
    {
        private readonly ListBox _logBox;

        internal Logger(ListBox listBox)
        {
            _logBox= listBox;
        }

        internal void AddLogEntry(string entry)
        {
            if (_logBox.Items.Count >= 1024)
            {
                SaveLogs();
                ClearLogs();
                _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} Logs have been saved and cleaned");
            }

            try
            {
                _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
            }
            catch
            {
                _logBox.Invoke((MethodInvoker) delegate
                {
                    _logBox.Items.Insert(0, $"{DateTime.UtcNow:G} {entry}");
                });
            }
            
        }

        internal void SaveLogs()
        {
            var logsFilePath = $"Logs {DateTime.UtcNow:yyyy-MM-ddTHH-mm-ss}.txt";
            var sw = new StreamWriter(logsFilePath);
            foreach (string item in _logBox.Items) sw.WriteLine(item);
            sw.Close();
        }

        internal void ClearLogs()
        {
            _logBox.Items.Clear();
        }

        internal void CopyLogItem()
        {
            if (_logBox.SelectedItems.Count > 0) Clipboard.SetText(_logBox.SelectedItem.ToString());
        }
    }
}