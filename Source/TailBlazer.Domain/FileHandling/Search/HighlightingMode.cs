using System.ComponentModel;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling.Search
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum HighlightingMode
    {
        [Description("No highlighting")]
        Disabled,
        [Description("Highlight text")]
        Text,
        [Description("Highlight line")]
        Line
    }
}