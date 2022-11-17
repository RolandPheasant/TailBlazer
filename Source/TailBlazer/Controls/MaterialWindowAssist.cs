using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TailBlazer.Controls
{
    public static class MaterialWindowAssist
    {
        public static readonly DependencyProperty LeftHeaderContentProperty = DependencyProperty.RegisterAttached(
            "LeftHeaderContent", typeof(object), typeof(MaterialWindowAssist), new PropertyMetadata(default(object)));

        public static void SetLeftHeaderContent(DependencyObject element, object value)
        {
            element.SetValue(LeftHeaderContentProperty, value);
        }

        public static object GetLeftHeaderContent(DependencyObject element)
        {
            return (object)element.GetValue(LeftHeaderContentProperty);
        }
    }
}
