using KenshiCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace KenshiCore.Mods
{
    public class ModItem
    {
        private readonly KenshiPaths paths;

        public string Name { get; set; }
        public string Language { get; set; } = "detecting...";
        public bool InGameDir { get; set; }
        public bool Selected { get; set; }
        public long WorkshopId { get; set; }
        public bool IsBaseGame { get; set; }

        public ModItem(string name, KenshiPaths? paths = null)
        {
            InGameDir = false;
            Selected = false;
            WorkshopId = -1;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            this.paths = paths ?? KenshiPaths.Empty;
        }
        public string getBackupFilePath()
        {
            return Path.Combine(Path.GetDirectoryName(getModFilePath())!, Path.GetFileNameWithoutExtension(Name) + ".backup");
        }
        public string getDictFilePath()
        {
            return Path.Combine(Path.GetDirectoryName(getModFilePath())!, Path.GetFileNameWithoutExtension(Name) + ".dict");
        }
        public string? getGamedirModPath()
        {
            if (InGameDir && !string.IsNullOrEmpty(paths.GameDirModsPath))
            {
                return Path.Combine(paths.GameDirModsPath, Path.GetFileNameWithoutExtension(Name), Name);
            }
            return null;
        }
        public string? getWorkshopModPath()
        {
            if (WorkshopId != -1 && !string.IsNullOrEmpty(paths.WorkshopModsPath))
            {
                return Path.Combine(paths.WorkshopModsPath, WorkshopId.ToString(), Name);
            }
            return null;
        }
        public string? getModFilePath()
        {
            if (IsBaseGame)
            {
                if (string.IsNullOrEmpty(paths.GameDirModsPath))
                    return null;
                string dataDir = Path.Combine(Path.GetDirectoryName(paths.GameDirModsPath)!, "data");
                return Path.Combine(dataDir, Name);
            }
            if (InGameDir)
            {
                return getGamedirModPath();
            }
            if (WorkshopId != -1)
            {
                return getWorkshopModPath();
            }
            return null;
        }
        public string? GetPatchTargetPath()
        {
                // Base game mods: patch in place
            if (IsBaseGame)
                throw new InvalidOperationException($"Don't patch base game mod, ever! '{Name}'");
            //return getModFilePath()!;

            // Already in game dir: patch in place
            if (InGameDir)
                return getGamedirModPath()!;

            // Workshop mod: must be copied first
            if (WorkshopId != -1 && !string.IsNullOrEmpty(paths.WorkshopModsPath) && !string.IsNullOrEmpty(paths.GameDirModsPath))
            {
                string workshopFolder = Path.Combine(
                    paths.WorkshopModsPath,
                    WorkshopId.ToString()
                );

                string gameDirFolder = Path.Combine(
                    paths.GameDirModsPath,
                    Path.GetFileNameWithoutExtension(Name)
                );

                string targetModPath = Path.Combine(gameDirFolder, Name);

                // Copy only if not already present
                if (!Directory.Exists(gameDirFolder))
                {
                    CoreUtils.Print($"Copying workshop mod '{Name}' to game dir");
                    CoreUtils.CopyDirectory(workshopFolder, gameDirFolder);
                    InGameDir = true;
                }

                return targetModPath;
            }
            UiHost.ShowMessage($"Cannot determine patch target for mod '{Name}'", "Error", UiIcon.Error);
            return null;
            //throw new InvalidOperationException($"Cannot determine patch target for mod '{Name}'");
        }
        public string? GetWorkshopDirectory() {
            if (WorkshopId == -1 || string.IsNullOrEmpty(paths.WorkshopModsPath))
                return null;
            return Path.Combine(paths.WorkshopModsPath, WorkshopId.ToString());
        }
        public string? getPatchPath()
        {
            string? modPath = getModFilePath();
            if (modPath == null)
                return null;
            string dir = Path.GetDirectoryName(modPath)!;
            string modName = Path.GetFileNameWithoutExtension(modPath);
            return Path.Combine(dir, modName + ".patch");
        }
    }
}
