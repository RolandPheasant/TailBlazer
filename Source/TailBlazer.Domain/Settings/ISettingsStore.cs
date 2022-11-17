namespace TailBlazer.Domain.Settings;

public interface ISettingsStore
{
    void Save(string key, State state);

    State Load(string key);
}