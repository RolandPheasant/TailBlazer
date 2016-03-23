namespace TailBlazer.Domain.Formatting
{
    public static class ThemeConstants
    {
        public const string LightThemeAccent = "indigo";
        public const string DarkThemeAccent = "yellow";

        public static readonly string[] Themes = {
            "yellow",
            "amber",
            "lightgreen",
            "green",
            "lime",
            "teal",
            "cyan",
            "lightblue",
            "blue",
            "indigo",
            "orange",
            "deeporange",
            "pink",
            "red",
            "purple",
            "deeppurple",
        };


        public static string GetAccentColor(this Theme theme)
        {
            return theme == Theme.Dark ? DarkThemeAccent : LightThemeAccent;
        }
    }
}