namespace Core.Utilities
{
    /// <summary>Icon variants surfaced by the UI layer. Kept out of System.Windows.Forms so the core stays UI-free.</summary>
    public enum UiIcon
    {
        None,
        Information,
        Warning,
        Error,
        Question
    }

    /// <summary>UI operations the core needs. The WinForms layer registers an implementation via <see cref="UiHost.Current"/>.</summary>
    public interface IUserInterface
    {
        string? PickFolder(string description);
        void ShowMessage(string message, string caption = "", UiIcon icon = UiIcon.None);
    }

    /// <summary>Static entry point for UI operations so the pure core has no direct WinForms dependency.</summary>
    public static class UiHost
    {
        public static IUserInterface? Current { get; set; }

        public static string? PickFolder(string description)
            => Current?.PickFolder(description);

        public static void ShowMessage(string message, string caption = "", UiIcon icon = UiIcon.None)
            => Current?.ShowMessage(message, caption, icon);
    }

    /// <summary>UI colors used by display methods, kept as an enum so the core avoids System.Drawing.</summary>
    public enum LogColor
    {
        Default,
        Orange,
        Gray,
        LightCyan,
        LightYellow,
        LightPink,
        LightSalmon,
        LightGray,
        Yellow,
        Red,
        LightBlue,
        LightGreen,
        Purple,
        Transparent,
        AliceBlue,
        IndianRed,
        White,
        Green,
        OrangeRed
    }

    /// <summary>A single colored, formatted log/display line.</summary>
    public readonly record struct LogEntry(string Text, LogColor Color);

    /// <summary>Long-running progress reporting. The WinForms layer registers an implementation via <see cref="ProgressHost.Current"/>.</summary>
    public interface IProgressReporter
    {
        void Initialize(int max);
        void Report(int value, string? text = null);
        void ReportStep(string? text = null);
        void Finish(string? text = null);
    }

    /// <summary>Static entry point for progress reporting so the pure core has no WinForms dependency.</summary>
    public static class ProgressHost
    {
        public static IProgressReporter? Current { get; set; }

        public static void Initialize(int max) => Current?.Initialize(max);
        public static void Report(int value, string? text = null) => Current?.Report(value, text);
        public static void ReportStep(string? text = null) => Current?.ReportStep(text);
        public static void Finish(string? text = null) => Current?.Finish(text);
    }
}
