using DynamicData;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Formatting
{
    public interface IIconProvider
    {
        IObservableList<IconDescription> Icons { get; }

        IKnownIcons KnownIcons { get; }
    }
}