using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IColourSelector
    {
        Hue SelectFor(string text);
    }
}