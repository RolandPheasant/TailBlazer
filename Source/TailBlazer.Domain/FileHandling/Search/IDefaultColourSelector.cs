using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IDefaultColourSelector
    {
        Hue Select(string text);
        Hue Lookup(HueKey key);
    }
}