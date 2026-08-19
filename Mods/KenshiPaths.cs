namespace Core.Mods
{
    /// <summary>
    /// Immutable snapshot of the resolved Kenshi installation paths. Owned and updated by
    /// <see cref="ModManager"/> and shared with <see cref="ModItem"/> and <see cref="ModRepository"/>,
    /// so models and repositories no longer read global static state.
    /// </summary>
    public sealed record KenshiPaths(
        string? SteamPath,
        string? KenshiPath,
        string? GameDirModsPath,
        string? WorkshopModsPath)
    {
        /// <summary>Represents an unconfigured installation (all paths null).</summary>
        public static readonly KenshiPaths Empty = new(null, null, null, null);
    }
}
