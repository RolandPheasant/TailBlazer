namespace TailBlazer.Domain.Settings
{
    public interface IConverter<T> 
    {
        T Convert(State state);
        State Convert(T state);

        T GetDefaultValue();
    }
}