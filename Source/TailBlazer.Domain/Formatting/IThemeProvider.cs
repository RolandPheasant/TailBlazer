using System;

namespace TailBlazer.Domain.Formatting
{
    public interface IThemeProvider
    {
        IObservable<Theme> Theme { get; }
        IObservable<Hue> Accent { get; }

    }
}