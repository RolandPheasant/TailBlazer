using System.Runtime.CompilerServices;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastucture
{
    public interface ILogFactory
    {
        ILogger Create(string name);
        ILogger Create<T>();
    }
}