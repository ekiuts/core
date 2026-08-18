using KenshiCore.ReverseEngineering;
using KenshiCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KenshiCore.Mods
{
    public class ModRepository
    {
        /// <summary>
        /// Service-locator bridge assigned once by the composition root. Application code should prefer an
        /// injected instance; this is provided for helper classes that are not wired for constructor injection.
        /// </summary>
        public static ModRepository? Current { get; set; }

        private readonly KenshiPaths paths;
        private readonly List<string> _baseGameMods = new();
        private readonly List<string> _gameDirMods = new();
        private readonly List<string> _workshopMods = new();
        private readonly List<string> _selectedMods = new();
        private Dictionary<string, string> AssetCache = new();

        public ModRepository(KenshiPaths paths)
        {
            this.paths = paths ?? KenshiPaths.Empty;
        }

        public IReadOnlyList<string> BaseGameMods => _baseGameMods;
        public IReadOnlyList<string> GameDirMods => _gameDirMods;
        public IReadOnlyList<string> WorkshopMods => _workshopMods;
        public IReadOnlyList<string> SelectedMods => _selectedMods;

        public bool excludeUnselectedMods = false;
        public void SetSelectedMods(List<string> mods)
        {
            _selectedMods.Clear();
            _selectedMods.AddRange(mods);
        }
        public void LoadBaseGameMods()//string gamedirDataPath)
        {
            if (string.IsNullOrEmpty(paths.KenshiPath))
                return;
            string gamedirDataPath = Path.Combine(paths.KenshiPath, "data");
            _baseGameMods.Clear();
            if (!Directory.Exists(gamedirDataPath)) return;

            foreach (var file in Directory.GetFiles(gamedirDataPath, "*.mod"))
                _baseGameMods.Add(Path.GetFileName(file));
            foreach (var file in Directory.GetFiles(gamedirDataPath, "*.base"))
                _baseGameMods.Add(Path.GetFileName(file));
            
        }
        
        public void LoadGameDirMods()//string modsPath)
        {
            if (string.IsNullOrEmpty(paths.GameDirModsPath))
                return;
            string modsPath = paths.GameDirModsPath;
            _gameDirMods.Clear();
            if (!Directory.Exists(modsPath)) return;

            foreach (var folder in Directory.GetDirectories(modsPath))
                _gameDirMods.AddRange(Directory.GetFiles(folder, "*.mod").Select(Path.GetFileName)!);
        }
        public void LoadWorkshopMods()//string workshopPath)
        {
            if (string.IsNullOrEmpty(paths.WorkshopModsPath))
                return;
            string workshopPath = paths.WorkshopModsPath;
            _workshopMods.Clear();
            if (!Directory.Exists(workshopPath)) return;

            foreach (var folder in Directory.GetDirectories(workshopPath))
            {
                _workshopMods.AddRange(Directory.GetFiles(folder, "*.mod")
                    .Select(f => Path.Combine(new DirectoryInfo(Path.GetDirectoryName(f)!).Name, Path.GetFileName(f))));
            }
        }
        public void LoadSelectedMods()//string cfgPath)
        {
            if (string.IsNullOrEmpty(paths.KenshiPath))
                return;
            string cfgPath = Path.Combine(paths.KenshiPath, "data", "mods.cfg");
            _selectedMods.Clear();
            if (!File.Exists(cfgPath)) return;

            foreach (var line in File.ReadAllLines(cfgPath))
            {
                if (!string.IsNullOrWhiteSpace(line))
                    _selectedMods.Add(line.Trim());
            }
        }
        public Dictionary<string, ModItem> GetMergedMods()
        {
            var merged = new Dictionary<string, ModItem>();

            // 1. Base game mods
            foreach (var mod in BaseGameMods)
            {
                if (!merged.ContainsKey(mod))
                    merged[mod] = new ModItem(mod, paths)
                    {
                        IsBaseGame = true,
                        Selected = true
                    };
            }
            // 2. Selected mods (mods.cfg)
            foreach (var mod in SelectedMods)
            {
                if (!merged.ContainsKey(mod))
                    merged[mod] = new ModItem(mod, paths);
                merged[mod].Selected = true;
            }

            // 3. GameDir mods
            foreach (var mod in GameDirMods)
            {
                if (!merged.ContainsKey(mod))
                    merged[mod] = new ModItem(mod, paths);
                merged[mod].InGameDir = true;
            }
            // 4. Workshop mods
            foreach (var folderMod in WorkshopMods)
            {
                string? folderPart = Path.GetDirectoryName(folderMod);
                if (folderPart == null) continue;

                string filePart = Path.GetFileName(folderMod);
                if (!merged.ContainsKey(filePart))
                    merged[filePart] = new ModItem(filePart, paths);

                string folderName = Path.GetFileName(folderPart);
                if (long.TryParse(folderName, out long workshopId))
                    merged[filePart].WorkshopId = workshopId;
            }
            // 5. Exclude unselected mods if the option is enabled
            if (excludeUnselectedMods) { 
                var unselectedKeys = merged.Where(kvp => !kvp.Value.Selected).Select(kvp => kvp.Key).ToList();
                foreach (var key in unselectedKeys)
                    merged.Remove(key);
            }
            Mods = merged;
            return merged;
        }
        public Dictionary<string, ModItem> FilterSelectedMods(Dictionary<string, ModItem> mods)
        {
            var selected = SelectedMods;
            return mods
                .Where(kvp => selected.Contains(kvp.Key, StringComparer.Ordinal))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
        public Dictionary<string, ModItem> Mods { get; private set; } = new();
        public string? getRealPathFromAsset(string assetname)
        {
            if (AssetCache.TryGetValue(assetname, out var cached))
                return cached;


            foreach (var last_mod in Mods.Values.Reverse())
            {
                string? modpath= Path.GetDirectoryName(last_mod.getModFilePath());
                if(modpath != null&&last_mod.Selected)
                {
                    var files = Directory.GetFiles(modpath, assetname, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        AssetCache[assetname] = files[0];
                        return files[0];
                    }
                }
            }
            return null;

        }
        public string ResolveRealPath(string virtualPath)
        {
            const string ISNULL = "E_ISNULL";
            const string NOTFOUND = "E_NOTFOUND";

            if (string.IsNullOrWhiteSpace(virtualPath))
                return ISNULL;

            string normalized = virtualPath.Replace('\\', '/');

            // BASE GAME FILE
            // ./data/animal/meshes/dog.mesh
            
            string? file = Path.GetFileName(virtualPath);
            if (file != null)
            {
                string? result = getRealPathFromAsset(file);
                if (result != null)
                {
                    return result;
                }
            }
            return NOTFOUND;
        }
    }
}
