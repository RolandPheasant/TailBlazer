namespace TailBlazer.Domain.Settings;

public class SettingsException: Exception
{
    public SettingsException(string message) : base(message)
    {
    }

    public SettingsException(string message, Exception innerException) : base(message, innerException)
    {
    }
}