using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    public class FileSegmentSearch
    {
        public FileSegmentKey Key { get; }
        public FileSegment Segment { get; }

        public FileSegmentSearchStatus Status { get; }

        public long[] Lines => _lines.Data;

        private readonly ImmutableList<long> _lines;
        

        public FileSegmentSearch(FileSegment segment, FileSegmentSearchStatus status = FileSegmentSearchStatus.Pending)
        {
            Key = segment.Key;
            Segment = segment;
            Status = status;
            _lines = new ImmutableList<long>();
        }

        public FileSegmentSearch(FileSegment segment, FileSearchResult result)
        {
            Key = segment.Key;
            Segment = segment;
            Status =  FileSegmentSearchStatus.Complete;
            _lines = new ImmutableList<long>(result.Indicies);
        }

        public FileSegmentSearch(FileSegmentSearch segmentSearch, FileSearchResult result)
        {
            //this can only be the tail as the tail will continue to grow
            Key = segmentSearch.Key;
            Segment = new FileSegment(segmentSearch.Segment, result.End); 
            Status = FileSegmentSearchStatus.Complete;
            _lines = segmentSearch._lines.Add(result.Indicies);
        }

        //  public 

    }
}