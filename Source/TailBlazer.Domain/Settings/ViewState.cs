using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.Settings;

public class ViewState
{
    public string Key { get;  }
    public State State { get; }

    public ViewState([NotNull] string key, State state=null)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        Key = key;
        State = state;
    }
}