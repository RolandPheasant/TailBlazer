using System;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.StateHandling;

public class StateBucket
{
    public string Type { get; }
    public string Id { get; }
    public State State { get; }
    public StateBucketKey Key { get;  }

    public DateTime TimeStamp { get; }

    public StateBucket(string type, string id, State state, DateTime timeStamp)
    {
        Key = new StateBucketKey(type,id);
        Type = type;
        Id = id;
        State = state;
        TimeStamp = timeStamp;
    }


}