using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndicies
    {
        public Encoding Encoding { get; }

        public SparseIndex[] Lines { get; }
        public int Count { get; }

        public int Diff { get; }
        public LinesChangedReason ChangedReason { get; }
        public int TailStartsAt { get; }


        public SparseIndicies(IReadOnlyCollection<SparseIndex> latest,
                                    SparseIndicies previous,
                                    Encoding encoding)
        {

            Encoding = encoding;
            TailStartsAt = latest.Select(idx => idx.End).Max();
            Count = latest.Select(idx => idx.LineCount).Sum();
            Lines = latest.ToArray();
            Diff = Count - (previous?.Count ?? 0);

            //need to check whether
            if (previous == null)
            {
                ChangedReason = LinesChangedReason.Loaded;

            }
            else
            {
                //check which notificaion has changed
       
                ChangedReason = LinesChangedReason.Tailed;
            }
        }

    }
}