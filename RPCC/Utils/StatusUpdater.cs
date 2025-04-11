using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RPCC.Utils;

public static class StatusUpdater
{
    private static readonly string statusPath = @"C:\Users\Администратор\RiderProjects\telescope-backend\status\status.json";
    private const string GlobalMutexName = "Global\\RoboPhotStatusFileLock";

    public static void UpdateNestedField(string[] path, JToken value)
    {
        using (var mutex = new Mutex(false, GlobalMutexName))
        {
            if (!mutex.WaitOne(5000))  // ждём до 5 секунд
                throw new IOException("Не удалось получить доступ к status.json (mutex timeout)");

            try
            {
                JObject root = File.Exists(statusPath)
                    ? JObject.Parse(File.ReadAllText(statusPath))
                    : new JObject();

                JObject current = root;
                for (int i = 0; i < path.Length - 1; i++)
                {
                    if (current[path[i]] == null || current[path[i]].Type != JTokenType.Object)
                        current[path[i]] = new JObject();

                    current = (JObject)current[path[i]];
                }

                current[path[path.Length - 1]] = value;
                root["last_update"] = DateTime.UtcNow.ToString("s");

                using (var fs = new FileStream(statusPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(fs))
                {
                    writer.Write(root.ToString(Formatting.Indented));
                }
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
    }
    

    public static void RunPreviewGenerator()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = "C:\\Users\\Администратор\\RiderProjects\\telescope-backend\\generate_latest_previews.py",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (var process = Process.Start(psi))
        {
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            // (опционально) логировать
            Console.WriteLine(output);
            Console.Error.WriteLine(error);
        }
        Logger.AddDebugLogEntry("Web previews updated");
    }
}