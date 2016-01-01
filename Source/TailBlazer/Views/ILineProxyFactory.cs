using TailBlazer.Domain.FileHandling;

namespace TailBlazer.Views
{
    public interface ILineProxyFactory
    {
        LineProxy Create(Line line);
    }
}