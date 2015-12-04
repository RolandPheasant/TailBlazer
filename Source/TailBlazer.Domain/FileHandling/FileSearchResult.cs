using System.Collections.Generic;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSearchResult
    {
        public long[] Matches { get; }
        public int TotalMatches => Matches.Length;
        public int SegmentsCompleted { get; }
        public int Segments { get; }
        public bool IsSearching { get; }

        public FileSearchResult(IEnumerable<FileSegmentSearch> segments)
        {
            var fileSegmentSearches = segments as FileSegmentSearch[] ?? segments.ToArray();
            IsSearching = fileSegmentSearches.Any(s => s.Status != FileSegmentSearchStatus.Complete);

            Segments = fileSegmentSearches.Length;
            SegmentsCompleted = fileSegmentSearches.Count(s=>s.Status== FileSegmentSearchStatus.Complete);
            Matches = fileSegmentSearches.SelectMany(s => s.Lines).ToArray();

        }


    }
}