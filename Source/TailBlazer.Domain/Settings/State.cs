namespace TailBlazer.Domain.Settings
{
    public class State
    {
        public int Version { get; }
        string Value { get; }

        public State(int version, string value)
        {
            Version = version;
            Value = value;
        }


    }
}
