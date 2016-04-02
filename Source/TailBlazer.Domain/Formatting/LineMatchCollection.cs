using System.ComponentModel;
using System.Linq;

namespace TailBlazer.Domain.Formatting
{
    public class LineMatchCollection: INotifyPropertyChanged
    {
        public static  readonly LineMatchCollection Empty = new LineMatchCollection(new LineMatch[0]);

        public int Count => Matches.Length;

        public LineMatch[] Matches { get; }
        
        public LineMatch FirstMatch { get; }

        public bool IsRegex => FirstMatch!=null && FirstMatch.UseRegex;
        public bool IsFilter => FirstMatch != null && !FirstMatch.UseRegex;
        public bool HasMatches => Matches.Length != 0;


        public event PropertyChangedEventHandler PropertyChanged;

        public LineMatchCollection(LineMatch[] matches)
        {
            Matches = matches;
            FirstMatch = matches.FirstOrDefault();
        }

    }
}