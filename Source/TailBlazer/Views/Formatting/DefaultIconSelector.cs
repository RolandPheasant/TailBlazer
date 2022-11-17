using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Formatting;

public sealed class DefaultIconSelector: IDefaultIconSelector
{
    private string RegEx { get; } = PackIconKind.Regex.ToString();
    private string Search { get; } = PackIconKind.Magnify.ToString();
        
    private IEnumerable<DefaultIcons> DefaultMatches { get; }

    public DefaultIconSelector()
    {
        DefaultMatches = LoadIcons().ToArray();
    }

    public string GetIconFor(string text, bool useRegex)
    {
        var match = DefaultMatches
            .FirstOrDefault(icon => icon.MatchTextOnCase
                ? icon.Text.Equals(text)
                : icon.Text.Equals(text, StringComparison.OrdinalIgnoreCase));

        return match != null ? match.IconName : useRegex ? RegEx : Search;
    }

    public string GetIconOrDefault(string text, bool useRegex, string iconKind)
    {
        var existing = DefaultMatches
            .FirstOrDefault(icon => icon.IconName.Equals(iconKind, StringComparison.OrdinalIgnoreCase));


        if (existing != null)
            return existing.IconName;

        return GetIconFor(text, useRegex);

    }

    private IEnumerable<DefaultIcons> LoadIcons()
    {
        yield return new DefaultIcons("DEBUG", PackIconKind.Bug.ToString());
        yield return new DefaultIcons("INFO", PackIconKind.InformationOutline.ToString());
        yield return new DefaultIcons("WARN", PackIconKind.AlertOutline.ToString());
        yield return new DefaultIcons("WARNING", PackIconKind.AlertOutline.ToString());
        yield return new DefaultIcons("ERROR", PackIconKind.SquareRootBox.ToString());
        yield return new DefaultIcons("FATAL", PackIconKind.ExitToApp.ToString());

        yield return new DefaultIcons("BANK", PackIconKind.Bank.ToString());
        yield return new DefaultIcons("PERSON", PackIconKind.Account.ToString());
        yield return new DefaultIcons("PEOPLE", PackIconKind.AccountMultiple.ToString());
        yield return new DefaultIcons("USD", PackIconKind.CurrencyUsd.ToString());
        yield return new DefaultIcons("GBP", PackIconKind.CurrencyGbp.ToString());
        yield return new DefaultIcons("EUR", PackIconKind.CurrencyEur.ToString());

        yield return new DefaultIcons("FUCK", PackIconKind.EmoticonDevil.ToString());
        yield return new DefaultIcons("SHIT", PackIconKind.EmoticonPoop.ToString());
        yield return new DefaultIcons("POOP", PackIconKind.EmoticonPoop.ToString());
        yield return new DefaultIcons("PISS", PackIconKind.EmoticonDevil.ToString());
        yield return new DefaultIcons("WANK", PackIconKind.EmoticonDevil.ToString());
    }

    private class DefaultIcons
    {
        public string Text { get; }
        public string IconName { get; }
        public bool MatchTextOnCase { get; }
            
        public DefaultIcons(string text, string iconName, bool matchTextOnCase = false)
        {
            Text = text;
            IconName = iconName;
            MatchTextOnCase = matchTextOnCase;
        }
    }
}