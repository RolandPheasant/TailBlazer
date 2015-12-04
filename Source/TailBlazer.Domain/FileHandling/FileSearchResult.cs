using System.Collections.Generic;
using System.Linq;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSearchResult
    {
        public static readonly FileSearchResult None = new FileSearchResult();
        public long[] Matches { get; }
        public int Total => Matches.Length;
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

        private FileSearchResult()
        {
            Matches = new long[0];
        }


    }
}