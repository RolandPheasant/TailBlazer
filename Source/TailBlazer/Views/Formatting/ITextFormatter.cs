using System;
using System.Collections.Generic;

namespace TailBlazer.Views.Formatting
{
    public interface ITextFormatter
    {
        IObservable<IEnumerable<DisplayText>> GetFormatter(string inputText);
    }
}