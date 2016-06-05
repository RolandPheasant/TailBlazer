using DynamicData.Kernel;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.StateHandling
{
    /// <summary>
    /// A simple means for dumping stuff to a file
    /// </summary>
    public interface IStateBucketService
    {
        void Write(string type, string id, State state);
        Optional<State> Lookup(string type, string id);
    }
}