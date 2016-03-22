using System.Collections.Generic;
using DynamicData.Kernel;

namespace TailBlazer.Domain.Formatting
{
    public interface IColourProvider
    {
        IEnumerable<Hue> Hues { get; }

        Hue DefaultAccent { get; }

        Optional<Hue> Lookup(HueKey key);

        Hue GetAccent(Theme theme);
    }
}