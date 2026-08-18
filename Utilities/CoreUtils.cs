using KenshiCore.Mods;
using System.Text;

namespace KenshiCore.Utilities
{
    public static class CoreUtils
    {
        public static Dictionary<string,bool> toggles { get; } = new Dictionary<string, bool>();

        public static bool isModPatched(ModItem mod)
        {
            string modpath = mod.getModFilePath()!;
            string dir = Path.GetDirectoryName(modpath)!;
            string modName = Path.GetFileNameWithoutExtension(modpath);
            string patchPath = Path.Combine(dir, modName + ".patch");
            string unpatchedPath = Path.Combine(dir, modName + ".unpatched");
            if (!File.Exists(patchPath))
                return false;
            return File.Exists(unpatchedPath);
        }
        public static bool isModAPatch(ModItem mod)
        {
            string modpath = mod.getModFilePath()!;
            string dir = Path.GetDirectoryName(modpath)!;
            string modName = Path.GetFileNameWithoutExtension(modpath);
            string patchPath = Path.Combine(dir, modName + ".patch");

            return File.Exists(patchPath);
        }
        public static bool isReKenshiMod(ModItem mod)
        {
            string modPath = mod.getModFilePath()!;
            string dir = Path.GetDirectoryName(modPath)!;

            return File.Exists(Path.Combine(dir, "RE_Kenshi.json"));
        }
        public static string getUnpatchedPath(ModItem mod)
        {
            string modpath = mod.getModFilePath()!;
            string dir = Path.GetDirectoryName(modpath)!;
            string modName = Path.GetFileNameWithoutExtension(modpath);
            string unpatchedPath = Path.Combine(dir, modName + ".unpatched");
            return unpatchedPath;
        }
        public static string? GetRealModPath(ModItem mod)
        {
            if (string.IsNullOrEmpty(mod.getModFilePath()))
                return null;

            if (isModPatched(mod))
                return getUnpatchedPath(mod);

            return mod.getModFilePath();
        }

        /// <summary>Raised for every printed/prompted message. Arguments are (message, id).</summary>
        public static event Action<string, int>? OnPrint
        {
            add { Logger.Logged += value; }
            remove { Logger.Logged -= value; }
        }

        public static void Print(string s, int id = -1) => Logger.Print(s, id, file: "");
        public static void Prompt(string s, int id = -1) => Logger.Prompt(s, id, file: "");
        public static void StartLog(string modName, string outputDir) => Logger.StartFileLog(modName, outputDir);
        public static void EndLog(string? summary = null) => Logger.EndFileLog(summary);

        public static void CopyDirectory(string sourceDir, string targetDir)
        {
            Directory.CreateDirectory(targetDir);

            foreach (var file in Directory.GetFiles(sourceDir))
                File.Copy(file, Path.Combine(targetDir, Path.GetFileName(file)), overwrite: true);

            foreach (var dir in Directory.GetDirectories(sourceDir))
                CopyDirectory(dir, Path.Combine(targetDir, Path.GetFileName(dir)));
        }
        public static List<string> SplitModList(string? modList)
        {
            if (string.IsNullOrWhiteSpace(modList))
                return new List<string>();

            var result = new List<string>();
            var parts = modList.Split(',');
            var currentPart = new StringBuilder();

            foreach (var part in parts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrWhiteSpace(trimmed))
                    continue;

                if (currentPart.Length > 0)
                    currentPart.Append(",");

                currentPart.Append(trimmed);

                if (IsCompleteModFilename(currentPart.ToString()))
                {
                    result.Add(currentPart.ToString());
                    currentPart.Clear();
                }
            }

            if (currentPart.Length > 0)
                result.Add(currentPart.ToString());

            return result
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }
        private static bool IsCompleteModFilename(string filename)
        {
            // A complete mod filename should end with .mod or .base
            // and have at least one character before the extension
            if (filename.EndsWith(".mod", StringComparison.Ordinal) ||
                filename.EndsWith(".base", StringComparison.Ordinal))
            {
                var withoutExtension = Path.GetFileNameWithoutExtension(filename);
                return !string.IsNullOrWhiteSpace(withoutExtension);
            }
            return false;
        }

        public static string AddModToList(string? modList, string modName, int hardStringLimit = 99999999)
        {
            if((modList?.Length ?? 0)+modName.Length+1 > hardStringLimit)
            {
                return modList ?? string.Empty;
            }
            if (string.IsNullOrWhiteSpace(modName))
                return modList ?? string.Empty;
            var mods = SplitModList(modList);
            if (string.Join(",", mods)!.Contains(",.base"))
            {
                UiHost.ShowMessage($"old Dependency:{string.Join(",", modList)} \nfound weird\n {modName}");
                UiHost.ShowMessage($"new Dependency:{string.Join(",", mods)} \nfound weird\n {modName}");
            }
            if (!mods.Any(d => d.Equals(modName, StringComparison.Ordinal)))
                mods.Add(modName);
            if (modName == ".base")
                UiHost.ShowMessage("found .base");
            
            return string.Join(",", mods);
        }

    }

}