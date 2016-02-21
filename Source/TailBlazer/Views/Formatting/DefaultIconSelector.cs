using System;
using System.Collections.Generic;
using System.Linq;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Formatting
{
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
        
        private IEnumerable<DefaultIcons> LoadIcons()
        {
            yield return new DefaultIcons("DEBUG", PackIconKind.Bug.ToString());
            yield return new DefaultIcons("INFO", PackIconKind.InformationOutline.ToString());
            yield return new DefaultIcons("WARN", PackIconKind.AlertOutline.ToString());
            yield return new DefaultIcons("WARNING", PackIconKind.AlertOutline.ToString());
            yield return new DefaultIcons("ERROR", PackIconKind.SquareInc.ToString());
            
            //yield return new DefaultIcons("CLOSE", PackIconKind.CloseCircleOutline.ToString(), false);
            //yield return new DefaultIcons("CLOSED", PackIconKind.CloseCircleOutline.ToString(), false);

            yield return new DefaultIcons("BANK", PackIconKind.Bank.ToString(), false);
            yield return new DefaultIcons("PERSON", PackIconKind.Account.ToString(), false);
            yield return new DefaultIcons("PEOPLE", PackIconKind.AccountMultiple.ToString(), false);

            yield return new DefaultIcons("USD", PackIconKind.CurrencyUsd.ToString(), false);
            yield return new DefaultIcons("GBP", PackIconKind.CurrencyGbp.ToString(), false);
            yield return new DefaultIcons("EUR", PackIconKind.CurrencyEur.ToString(), false);


            yield return new DefaultIcons("FUCK", PackIconKind.EmoticonDevil.ToString(), false);
            yield return new DefaultIcons("SHIT", PackIconKind.EmoticonDevil.ToString(), false);
            yield return new DefaultIcons("PISS", PackIconKind.EmoticonDevil.ToString(), false);
            yield return new DefaultIcons("WANK", PackIconKind.EmoticonDevil.ToString(), false);
        }
    }


}