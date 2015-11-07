namespace Trader.Domain.Infrastucture
{
    public interface IObjectProvider
    {
        T Get<T>();
    }
}
