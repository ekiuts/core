using Core.Mods;
using Core.Utilities;
using System.Drawing;

namespace Core.UI
{
    /// <summary>Icon rendering for <see cref="ModItem"/>. Lives in the UI layer because it depends on System.Drawing.</summary>
    public static class ModItemIcon
    {
        private static readonly Dictionary<int, Image> iconCache = new();

        public static Image? GameDirIcon { get; } = ResourceLoader.LoadImage("Core.icons.kenshiicon.png");
        public static Image? WorkshopIcon { get; } = ResourceLoader.LoadImage("Core.icons.steamicon.png");
        public static Image? SelectedIcon { get; } = ResourceLoader.LoadImage("Core.icons.selectedicon.png");

        public static Image CreateCompositeIcon(this ModItem mod)
        {
            if (mod.IsBaseGame)
            {
                using (Bitmap tempBmp = new Bitmap(48, 16))
                using (Graphics g = Graphics.FromImage(tempBmp))
                {
                    g.DrawImage(GameDirIcon!, 0, 0);
                    g.DrawImage(GameDirIcon!, 16, 0);
                    g.DrawImage(GameDirIcon!, 32, 0);
                    return (Image)tempBmp.Clone();
                }
            }

            int key = (Convert.ToInt32(mod.InGameDir) * 100) +
                      (Convert.ToInt32(mod.WorkshopId != -1) * 10) +
                      Convert.ToInt32(mod.Selected);

            if (iconCache.TryGetValue(key, out var cached))
                return cached;

            using (Bitmap blank = new Bitmap(16, 16))
            using (Bitmap tempBmp = new Bitmap(48, 16))
            using (Graphics g = Graphics.FromImage(tempBmp))
            {
                g.DrawImage(mod.InGameDir ? GameDirIcon! : blank, 0, 0);
                g.DrawImage(mod.WorkshopId != -1 ? WorkshopIcon! : blank, 16, 0);
                g.DrawImage(mod.Selected ? SelectedIcon! : blank, 32, 0);

                Image finalImage = (Image)tempBmp.Clone();
                iconCache[key] = finalImage;
                return finalImage;
            }
        }

        public static void DisposeIconCache()
        {
            foreach (var image in iconCache.Values)
                image.Dispose();
            iconCache.Clear();
        }
    }
}
