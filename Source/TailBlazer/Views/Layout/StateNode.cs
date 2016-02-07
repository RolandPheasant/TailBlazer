
using System.Collections.Generic;

namespace TailBlazer.Views.Layout
{
    public class StateNode
    {
        public object Content { get; set; }

        public List<StateNode> Children { get; } = new List<StateNode>();
    }
}
