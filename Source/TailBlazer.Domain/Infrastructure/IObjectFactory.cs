using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.Infrastructure;

public interface IObjectProvider
{
    T Get<T>();
    T Get<T>(NamedArgument arg);
    T Get<T>(IEnumerable<NamedArgument> args);
    T Get<T>(IArgument arg);
    T Get<T>(IEnumerable<IArgument> args);
}

public interface IObjectRegister
{
    void Register<T>(T instance) where T : class;
}
    
public class NamedArgument
{
    public string Key { get; }
    public object Instance { get; }

    public NamedArgument([NotNull] string key, [NotNull] object instance)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        if (instance == null) throw new ArgumentNullException(nameof(instance));
        Key = key;
        Instance = instance;
    }
}

public interface IArgument
{
    Type TargetType { get; }
    object Value { get; }
}

public class Argument : IArgument
{
    public object Value { get; }

    public Type TargetType { get; }

    public Argument([NotNull] object value, Type registerAs = null)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        Value = value;
        TargetType = registerAs ?? value.GetType();
    }
}

public class Argument<T>: IArgument
{
    private T Value { get; }

    public Argument([NotNull] T value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        Value = value;
    }

    public Type TargetType => typeof(T);

    object IArgument.Value => Value;
}