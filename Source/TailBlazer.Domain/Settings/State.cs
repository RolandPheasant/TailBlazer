using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TailBlazer.Domain.Persistence
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
