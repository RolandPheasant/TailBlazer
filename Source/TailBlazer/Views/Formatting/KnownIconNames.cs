using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DynamicData.Kernel;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Formatting
{
    public sealed class KnownIconNames: IKnownIconNames
    {
        public string Selected { get; } = PackIconKind.ArrowRightBold.ToString();

        public string RegEx { get; } = PackIconKind.Regex.ToString();
        public string Search { get; } = PackIconKind.Magnify.ToString();

        private IDictionary<string,string> Defaults { get; } = new Dictionary<string, string>();
        private IEnumerable<DefaultIcons> DefaultMatches { get; } 

        public string SelectIconFor(string text, bool useRegex)
        {
            var match = DefaultMatches
                .FirstOrDefault(icon => icon.MatchTextOnCase
                    ? icon.Text.Equals(text)
                    : icon.Text.Equals(text, StringComparison.OrdinalIgnoreCase));

            return match != null ? match.IconName : useRegex ? RegEx : Search;
        }

        public KnownIconNames()
        {
            DefaultMatches = LoadIcons().ToArray();

            DefaultMatches.ForEach(icon =>
            {
                Defaults[icon.Text] = icon.IconName;
            });
        }

        private IEnumerable<DefaultIcons> LoadIcons()
        {
            yield return new DefaultIcons("DEBUG", PackIconKind.Bug.ToString());
            yield return new DefaultIcons("INFO", PackIconKind.InformationOutline.ToString());
            yield return new DefaultIcons("WARN", PackIconKind.AlertOutline.ToString());
            yield return new DefaultIcons("ERROR", PackIconKind.SquareInc.ToString());

            yield return new DefaultIcons("USD", PackIconKind.CurrencyUsd.ToString(), false);
            yield return new DefaultIcons("GBP", PackIconKind.CurrencyGbp.ToString(), false);
            yield return new DefaultIcons("EUR", PackIconKind.CurrencyEur.ToString(), false);
        }
    }


}