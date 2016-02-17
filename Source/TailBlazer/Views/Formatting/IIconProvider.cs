using DynamicData;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Formatting
{
    public interface IIconProvider
    {
        IObservableList<IconDescription> Icons { get; }
    }
}