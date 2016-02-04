using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TailBlazer.Views.Layout
{
    public class TreeNode
    {
        public object Content { get; set; }

        public List<TreeNode> Children { get; } = new List<TreeNode>();
    }
}
