namespace TailBlazer.Domain.Infrastructure
{
    public interface IObjectProvider
    {
        T Get<T>();
        T Get<T>(ExplictArg explictArg);
    }

    public class ExplictArg
    {
        public string Key { get; }
        public object Arg { get; }

        public ExplictArg(string key, object arg)
        {
            Key = key;
            Arg = arg;
        }
    }
}
