namespace TailBlazer.Domain.Persistence
{
    public interface ISettingsStore
    {
        void Save(string key, State state);
        State Load(string key);
    }
}