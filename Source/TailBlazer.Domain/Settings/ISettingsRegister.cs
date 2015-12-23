using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.Settings
{
    public interface ISettingsRegister
    {
        void Register<T>([NotNull] IConverter<T> converter, [NotNull] string key);
    }
}