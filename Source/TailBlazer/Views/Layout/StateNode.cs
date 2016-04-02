
using System.Collections.Generic;

namespace TailBlazer.Views.Layout
{
    public class StateNode
    {
        public object Content { get;  }

        public List<StateNode> Children { get; } = new List<StateNode>();


        public StateNode(object content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return $"{Content} ({1} children)";
        }
    }
}
