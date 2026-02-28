using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace RPCC.Utils;

public static class StatusUpdater
{
    private static readonly string StatusPath = @"C:\Users\Администратор\RiderProjects\telescope-backend\status\status.json";
    // private static readonly string StatusPath = Path.Combine(Settings.MainOutputFolder, "status", "status.json");
    private const string GlobalMutexName = "Global\\RoboPhotStatusFileLock";
    private static int _previewRunning = 0;
    /// <summary>
    /// Обновляет значение по вложенному пути, например ["dome", "south_shutter", "position"]
    /// </summary>
    public static void UpdateNestedField(string[] path, JToken value)
    {
        using var mutex = new Mutex(false, GlobalMutexName);
        try
        {
            mutex.WaitOne();

            var root = JsonHelper.LoadJsonSafely(StatusPath);
            if (root == null)
            {
                Logger.AddLogEntry("[StatusUpdater] Пропущено обновление JSON — не удалось загрузить.");
                return;
            }

            // Спускаемся по дереву до нужного узла
            JToken current = root;
            for (int i = 0; i < path.Length - 1; i++)
            {
                if (current[path[i]] == null)
                    current[path[i]] = new JObject();

                current = current[path[i]];
            }

            // Обновляем целевое поле
            string lastKey = path[path.Length - 1];
            current[lastKey] = value;

            // Обновляем время последнего изменения
            root["last_update"] = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");

            // Сохраняем обратно в файл
            JsonHelper.SaveJsonToFile(StatusPath, root);
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }


    /// <summary>
    /// Обновляет поле верхнего уровня, например "mount"
    /// </summary>
    public static void UpdateRootField(string field, JToken value)
    {
        UpdateNestedField(new[] { field }, value);
    }
    
    internal static void RunPreviewGeneratorAsync()
    {
        // Prevent parallel preview generation (coalescing)
        if (Interlocked.Exchange(ref _previewRunning, 1) == 1)
            return;

        _ = Task.Run(() =>
        {
            try
            {
                RunPreviewGeneratorInternal();
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry($"Preview generator error: {ex}");
            }
            finally
            {
                Interlocked.Exchange(ref _previewRunning, 0);
            }
        });
    }

    // Старая логика перенесена сюда без изменений
    private static void RunPreviewGeneratorInternal()
    {
        Logger.AddDebugLogEntry("Web previews updating...");
        var psi = new ProcessStartInfo
        {
            FileName = "C:\\Program Files\\Python311\\python.exe",
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