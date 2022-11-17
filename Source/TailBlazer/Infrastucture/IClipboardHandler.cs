using System.Collections.Generic;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Infrastucture;

public interface IClipboardHandler
{
    void WriteToClipboard([NotNull] string text);
    void WriteToClipboard([NotNull] IEnumerable<string> items);
}