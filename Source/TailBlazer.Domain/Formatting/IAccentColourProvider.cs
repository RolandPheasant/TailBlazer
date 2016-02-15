using System.Collections.Generic;

namespace TailBlazer.Domain.Formatting
{
    public interface IAccentColourProvider
    {
        IEnumerable<Hue> Hues { get; }
        Hue DefaultHighlight { get; }
    }
}