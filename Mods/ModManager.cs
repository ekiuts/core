using Core.Utilities;
using Microsoft.Win32;

namespace Core.Mods
{
    /// <summary>
    /// Resolves and owns the Kenshi installation paths. Instance-based: the concrete
    /// <see cref="Paths"/> are immutable and injected repositories/models receive them,
    /// so no global mutable state is used.
    /// </summary>
    public class ModManager
    {
        /// <summary>
        /// Service-locator bridge assigned once by the composition root. Application code should prefer an
        /// injected instance; this is provided for helper classes that are not wired for constructor injection.
        /// </summary>
        public static ModManager? Current { get; set; }

        private readonly AppConfig config;
        private readonly IUserInterface? ui;

        /// <summary>The resolved installation paths. Updated via the Set/Manage methods.</summary>
        public KenshiPaths Paths { get; private set; } = KenshiPaths.Empty;

        public string? SteamPath => Paths.SteamPath;
        public string? KenshiPath => Paths.KenshiPath;
        public string? GameDirModsPath => Paths.GameDirModsPath;
        public string? WorkshopModsPath => Paths.WorkshopModsPath;

        public ModManager(AppConfig? config = null, IUserInterface? ui = null)
        {
            this.config = config ?? AppConfig.Load();
            this.ui = ui;
            SolvePaths();
        }

        private string? PickFolder(string description)
            => ui?.PickFolder(description) ?? UiHost.PickFolder(description);

        private void ShowMessage(string message)
        {
            if (ui != null)
                ui.ShowMessage(message);
            else
                UiHost.ShowMessage(message);
        }

        private string FindSteamInstallPath()
        {
            string? steamPath = string.Empty;
            if (OperatingSystem.IsWindows())
            {
                steamPath =
                    Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null)
                    as string
                    ?? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Valve\Steam", "InstallPath", null)
                    as string
                    ?? Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Wow6432Node\Valve\Steam", "InstallPath", null)
                    as string
                    ?? string.Empty;
            }

            if (string.IsNullOrEmpty(steamPath))
                return string.Empty;

            // Normalize path if registry points to steamapps
            if (Path.GetFileName(steamPath)
                .Equals("steamapps", StringComparison.OrdinalIgnoreCase))
            {
                var parent = Directory.GetParent(steamPath);
                if (parent != null)
                    steamPath = parent.FullName;
            }

            return steamPath;
        }

        private string? FindKenshiInstallDir(string steamPath)
        {
            if (!string.IsNullOrEmpty(steamPath))
            {
                string defaultPath = Path.Combine(steamPath, "steamapps", "common", "Kenshi");
                if (Directory.Exists(defaultPath))
                    return defaultPath;
            }
            string? folder = PickFolder("Please select your Kenshi installation folder (it should contain data/mods.cfg).");
            if (!string.IsNullOrEmpty(folder) && File.Exists(Path.Combine(folder, "data", "mods.cfg")))
                return folder;

            ShowMessage("That folder doesn’t look like a Kenshi install (mods.cfg not found).");
            return null;
        }

        public void SolvePaths()
        {
            string steamPath = config.SteamPath ?? FindSteamInstallPath();
            string? kenshiPath = config.KenshiPath ?? FindKenshiInstallDir(steamPath);
            if (string.IsNullOrEmpty(kenshiPath))
            {
                ShowMessage("Kenshi installation not found!\n Please set it manually by clicking the \"Browse\" button.");
                Paths = new KenshiPaths(string.IsNullOrEmpty(steamPath) ? null : steamPath, null, null, null);
                return;
            }

            string gameDirModsPath = Path.Combine(kenshiPath, "mods");
            string? workshopModsPath = string.IsNullOrEmpty(steamPath)
                ? null
                : Path.Combine(steamPath, "steamapps", "workshop", "content", "233860");

            Paths = new KenshiPaths(steamPath, kenshiPath, gameDirModsPath, workshopModsPath);
        }

        public void SetManualSteamPath(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

            string workshopModsPath = Path.Combine(path, "steamapps", "workshop", "content", "233860");
            Paths = new KenshiPaths(path, Paths.KenshiPath, Paths.GameDirModsPath, workshopModsPath);
            config.SteamPath = path;
            config.Save();
        }

        public void SetManualKenshiPath(string path)
        {
            if (!Directory.Exists(path)) throw new DirectoryNotFoundException(path);

            string gameDirModsPath = Path.Combine(path, "mods");
            string? workshopModsPath = string.IsNullOrEmpty(Paths.SteamPath)
                ? null
                : Path.Combine(Paths.SteamPath, "steamapps", "workshop", "content", "233860");

            Paths = new KenshiPaths(Paths.SteamPath, path, gameDirModsPath, workshopModsPath);
            config.KenshiPath = path;
            config.Save();
        }

        public bool TrySetKenshiPath(string path, out string errorMessage)
        {
            errorMessage = "";
            if (!Directory.Exists(path))
            {
                errorMessage = "Selected folder does not exist.";
                return false;
            }

            bool hasExe = File.Exists(Path.Combine(path, "kenshi.exe")) || File.Exists(Path.Combine(path, "kenshi_x64.exe"));
            bool hasData = Directory.Exists(Path.Combine(path, "data"));

            if (!hasExe || !hasData)
            {
                errorMessage = "That folder doesn’t look like a Kenshi install (kenshi.exe or data/ missing).";
                return false;
            }

            SetManualKenshiPath(path);
            return true;
        }

        public bool PromptAndSetKenshiPath()
        {
            string? folder = PickFolder("Please select your Kenshi installation folder (it should contain kenshi.exe and data/).");
            if (string.IsNullOrEmpty(folder))
                return false;

            return TrySetKenshiPath(folder, out _);
        }
    }
}
