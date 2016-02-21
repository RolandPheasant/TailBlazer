using System.Collections.Generic;

namespace TailBlazer.Domain.Formatting
{
    public interface IColourProvider
    {
        IEnumerable<Hue> Hues { get; }
        Hue DefaultHighlight { get; }
    }
}