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
        private readonly ObservableCollection<TreeNode> _children = new ObservableCollection<TreeNode>();

        public object Content { get; set; }

        public ObservableCollection<TreeNode> Children
        {
            get { return _children; }
        }
    }
}
