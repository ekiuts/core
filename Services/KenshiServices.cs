using Core.Mods;
using Core.ReverseEngineering;
using Core.Utilities;

namespace Core
{
    /// <summary>
    /// Composition root for the Kenshi tooling services. Constructed once at app startup and injected
    /// into forms/consumers. Removes the previous global singletons and mutable static state.
    /// </summary>
    public class KenshiServices
    {
        public ModManager ModManager { get; }
        public ModRepository ModRepository { get; }
        public ReverseEngineerRepository ReverseEngineerRepository { get; }
        public FileAnalyzer FileAnalyzer { get; }

        public KenshiServices(AppConfig? config = null, IUserInterface? ui = null)
        {
            ModManager = new ModManager(config, ui);
            ModRepository = new ModRepository(ModManager.Paths);
            ReverseEngineerRepository = new ReverseEngineerRepository();
            FileAnalyzer = new FileAnalyzer();

            // Service-locator bridges for code paths (static ModRecord getters, helper classes) that
            // are not wired for constructor injection.
            ModManager.Current = ModManager;
            ModRepository.Current = ModRepository;
            ReverseEngineerRepository.Current = ReverseEngineerRepository;
            FileAnalyzer.Current = FileAnalyzer;
        }

        /// <summary>Reload the mod list backing the repositories from the resolved install paths.</summary>
        public void LoadMods()
        {
            ModRepository.LoadBaseGameMods();
            ModRepository.LoadGameDirMods();
            ModRepository.LoadWorkshopMods();
            ModRepository.LoadSelectedMods();
        }
    }
}
