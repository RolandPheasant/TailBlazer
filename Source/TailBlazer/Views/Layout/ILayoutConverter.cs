using System.Xml.Linq;

namespace TailBlazer.Views.Layout
{
    public interface ILayoutConverter
    {
        XElement CaptureState();
    }
}