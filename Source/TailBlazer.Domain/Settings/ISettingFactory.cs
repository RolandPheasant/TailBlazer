namespace TailBlazer.Domain.Settings
{
    public interface ISettingFactory
    {
        ISetting<T> Create<T>(IConverter<T> converter, string key);
    }
}