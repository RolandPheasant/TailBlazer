using System;
using System.Collections.Generic;
using DynamicData.Kernel;

namespace TailBlazer.Domain.Formatting
{
    public interface IColourProvider
    {
        IEnumerable<Hue> Hues { get; }

        IObservable<Hue> AccentChanged { get; }

        Hue DefaultAccent { get; }

        Optional<Hue> Lookup(HueKey key);
    }
}