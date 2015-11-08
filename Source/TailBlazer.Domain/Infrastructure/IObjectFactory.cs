namespace TailBlazer.Domain.Infrastructure
{
    public interface IObjectProvider
    {
        T Get<T>();
    }
}
