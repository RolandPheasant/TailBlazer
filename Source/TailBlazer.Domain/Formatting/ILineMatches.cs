using System;
using System.Collections.Generic;

namespace TailBlazer.Domain.Formatting
{
    public interface ILineMatches
    {
        IObservable<LineMatchCollection> GetMatches(string inputText);
    }
}