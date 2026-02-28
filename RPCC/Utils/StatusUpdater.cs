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
    private const int MutexWaitTimeoutMs = 2000;
    private const int PreviewProcessTimeoutMs = 20000;
    /// <summary>
    /// Обновляет значение по вложенному пути, например ["dome", "south_shutter", "position"]
    /// </summary>
    public static void UpdateNestedField(string[] path, JToken value)
    {
        using var mutex = new Mutex(false, GlobalMutexName);
        var hasMutex = false;
        try
        {
            // mutex.WaitOne();
            hasMutex = mutex.WaitOne(MutexWaitTimeoutMs);
            if (!hasMutex)
            {
                Logger.AddLogEntry($"[StatusUpdater] Mutex timeout ({MutexWaitTimeoutMs} ms). Пропущено обновление JSON.");
                return;
            }
            
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
            // mutex.ReleaseMutex();
            if (hasMutex)
            {
                try { mutex.ReleaseMutex(); } catch { /* ignore */ }
            }
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
        
        // using (var process = Process.Start(psi))
        // {
        //     string output = process.StandardOutput.ReadToEnd();
        //     string error = process.StandardError.ReadToEnd();
        //     process.WaitForExit();
        //
        //     // (опционально) логировать
        //     Console.WriteLine(output);
        //     Console.Error.WriteLine(error);
        // }
        
        using var process = Process.Start(psi);
        if (process == null)
        {
            Logger.AddLogEntry("[StatusUpdater] Preview generator: Process.Start returned null.");
            return;
        }

        // Read stdout/stderr concurrently; avoid pipe deadlocks.
        Task<string> stdoutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stderrTask = process.StandardError.ReadToEndAsync();

        // Enforce an upper bound on how long we wait.
        var exited = process.WaitForExit(PreviewProcessTimeoutMs);
        if (!exited)
        {
            try
            {
                Logger.AddLogEntry($"[StatusUpdater] Preview generator timeout ({PreviewProcessTimeoutMs} ms). Killing process.");
                process.Kill();
            }
            catch (Exception ex)
            {
                Logger.AddLogEntry($"[StatusUpdater] Preview generator Kill() failed: {ex.Message}");
            }
        }

        string output = "";
        string error = "";
        try
        {
            // Ensure reads complete even if the process exited quickly.
            Task.WaitAll(new Task[] { stdoutTask, stderrTask }, 2000);
            if (stdoutTask.Status == TaskStatus.RanToCompletion)
                output = stdoutTask.Result;
            if (stderrTask.Status == TaskStatus.RanToCompletion)
                error = stderrTask.Result;
        }
        catch
        {
            // ignore read aggregation issues; process may have been killed mid-stream
        }

        if (!string.IsNullOrWhiteSpace(output))
            Logger.AddDebugLogEntry($"[StatusUpdater] Preview generator stdout:\n{output}");
        if (!string.IsNullOrWhiteSpace(error))
            Logger.AddLogEntry($"[StatusUpdater] Preview generator stderr:\n{error}");
        Logger.AddDebugLogEntry("Web previews updated");
    }
}