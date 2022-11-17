namespace TailBlazer.Domain.FileHandling;

public interface IProgressInfo
{
    int SegmentsCompleted { get; }
    int Segments { get; }
    bool IsSearching { get; }
}