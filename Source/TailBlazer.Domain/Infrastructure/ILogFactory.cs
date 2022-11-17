namespace TailBlazer.Domain.Infrastructure;

public interface ILogFactory
{
    ILogger Create(string name);
    ILogger Create<T>();
}