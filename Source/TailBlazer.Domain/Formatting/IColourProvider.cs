using System.Collections.Generic;
using DynamicData.Kernel;

namespace TailBlazer.Domain.Formatting
{
    public interface IColourProvider
    {
        IEnumerable<Hue> Hues { get; }
        Hue DefaultHighlight { get; }

        Optional<Hue> Lookup(HueKey key);
    }
}