using TailBlazer.Domain.Formatting;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IDefaultColourSelector
    {
        Hue Select(string text);
    }
}