using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace KenshiCore.Utilities
{
    /// <summary>
    /// The single logging facade for the core. Owns output suppression (by file or id),
    /// the in-process <see cref="Logged"/> event, and the optional file log.
    /// <see cref="CoreUtils"/> delegates its logging helpers here.
    /// </summary>
    public static class Logger
    {
        private static readonly object _lock = new object();
        private static readonly HashSet<string> mutedFiles = new(StringComparer.OrdinalIgnoreCase);
        private static readonly HashSet<int> shushedIds = new();

        private static StreamWriter? _logWriter;
        private static bool _fileLogEnabled;

        /// <summary>Raised for every printed or prompted message. Arguments are (message, id).</summary>
        public static event Action<string, int>? Logged;

        /// <summary>Suppress all output originating from a given source file name.</summary>
        public static void Mute(string fileName) => mutedFiles.Add(fileName);

        /// <summary>Suppress all output carrying the given log id.</summary>
        public static void Shush(int id) => shushedIds.Add(id);

        public static void Print(
            string message,
            int id = -1,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            if (IsSuppressed(id, file))
                return;
            Write(message, id, file, line);
        }

        public static void Prompt(
            string message,
            int id = -1,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            if (IsSuppressed(id, file))
                return;
            UiHost.ShowMessage(message);
            Write(message, id, file, line);
        }

        /// <summary>Begin writing subsequent log output to <c>{outputDir}/{modName}_patch.log</c>.</summary>
        public static void StartFileLog(string modName, string outputDir)
        {
            try
            {
                Directory.CreateDirectory(outputDir);
                _logWriter?.Dispose();

                var stream = new FileStream(
                    Path.Combine(outputDir, $"{modName}_patch.log"),
                    FileMode.Create, FileAccess.Write, FileShare.ReadWrite);

                _logWriter = new StreamWriter(stream, Encoding.UTF8);
                _fileLogEnabled = true;

                WriteDirect($"[{DateTime.Now}] Starting patch for {modName}");
                WriteDirect(new string('-', 60));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Logger] Failed to start log: {ex.Message}");
                _fileLogEnabled = false;
            }
        }

        /// <summary>Flush and close the file log started by <see cref="StartFileLog"/>.</summary>
        public static void EndFileLog(string? summary = null)
        {
            if (!_fileLogEnabled) return;

            lock (_lock)
            {
                WriteDirect(new string('-', 60));
                if (!string.IsNullOrWhiteSpace(summary))
                    WriteDirect(summary);
                WriteDirect($"[{DateTime.Now}] Patch process finished");
                _logWriter?.Flush();
                _logWriter?.Close();
                _logWriter = null;
                _fileLogEnabled = false;
            }
        }

        private static bool IsSuppressed(int id, string file)
        {
            if (shushedIds.Contains(id))
                return true;
            return !string.IsNullOrEmpty(file) && mutedFiles.Contains(Path.GetFileName(file));
        }

        private static void Write(string message, int id, string file, int line)
        {
            System.Diagnostics.Debug.WriteLine(message);
            Logged?.Invoke(message, id);

            if (_fileLogEnabled)
            {
                string source = string.IsNullOrEmpty(file)
                    ? ""
                    : $"[{Path.GetFileName(file)}:{line}] ";
                WriteDirect($"[{DateTime.Now:HH:mm:ss}] {source}{message}");
            }
        }

        private static void WriteDirect(string msg)
        {
            if (!_fileLogEnabled || _logWriter == null) return;
            lock (_lock)
            {
                _logWriter.WriteLine(msg);
                _logWriter.Flush();
            }
        }
    }
}
