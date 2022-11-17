using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views.Tail;

public interface ILineProxyFactory
{
    LineProxy Create(Line line);
}