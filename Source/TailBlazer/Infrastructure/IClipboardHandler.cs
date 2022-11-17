using TailBlazer.Domain.Annotations;

namespace TailBlazer.Infrastructure;

public interface IClipboardHandler
{
    void WriteToClipboard([NotNull] string text);
    void WriteToClipboard([NotNull] IEnumerable<string> items);
}