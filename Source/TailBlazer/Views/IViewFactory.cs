using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TailBlazer.Views
{
   // public interface 

    public interface IViewModelFactory
    {
        string Key { get; }

        string Description { get; }

    }
}
