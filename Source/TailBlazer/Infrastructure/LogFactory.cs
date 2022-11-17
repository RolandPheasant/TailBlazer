using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastructure;

public class LogFactory : ILogFactory
{
    public ILogger Create(string name)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        return new Log4NetLogger(name);
    }
    public ILogger Create<T>()
    {
        return new Log4NetLogger(typeof(T));
    }
}