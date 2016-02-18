using System.Collections.Generic;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.FileHandling.Search;

namespace TailBlazer.Views.Formatting
{
    public sealed class KnownIconNames: IKnownIconNames
    {
        public string Selected { get; } = PackIconKind.ArrowRightBold.ToString();

        public string RegEx { get; } = PackIconKind.Regex.ToString();
        public string Search { get; } = PackIconKind.Magnify.ToString();

        public IEnumerable<DefaultIcons> DefaultIcons()
        {
         //   yield return new DefaultIcons("S", Info);
            yield return new DefaultIcons("INFO", PackIconKind.Information.ToString());
            yield return new DefaultIcons("DEBUG", PackIconKind.Bug.ToString());
            yield return new DefaultIcons("ERROR", PackIconKind.ThumbDown.ToString());


        }



    }


}