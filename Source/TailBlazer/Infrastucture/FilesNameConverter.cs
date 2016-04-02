using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace TailBlazer.Infrastucture
{
    public class FilesNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var s = value as IEnumerable<string>;
            if (s == null)
            {
                throw new ArgumentException(nameof(value));
            }
            if (s.Count() == 1)
            {
                return s.ElementAt(0);
            }
            return "Tailed files";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
