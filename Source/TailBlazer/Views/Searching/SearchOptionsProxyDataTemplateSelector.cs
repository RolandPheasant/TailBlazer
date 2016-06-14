using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TailBlazer.Views.Searching
{
    public class SearchOptionsProxyDataTemplateSelector: DataTemplateSelector
    {
        public DataTemplate ExcludeDataTemplate { get; set; }
        public DataTemplate DefaultDataTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var proxy = (SearchOptionsProxy) item;
            return proxy != null && proxy.IsExclusion ? ExcludeDataTemplate : DefaultDataTemplate;
        }
    }
}
