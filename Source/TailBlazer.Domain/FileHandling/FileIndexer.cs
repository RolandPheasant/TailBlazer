using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class FileIndexer
    {
        private readonly IObservable<FileSegmentReport> _fileSegments;
        private readonly int _compression;
        private readonly int _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing;

        public IObservable<FileIndexCollection> Result { get; }

        public FileIndexer([NotNull] IObservable<FileSegmentReport> fileSegments,
            int compression = 10,
            int sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing = 250000000)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));
            _fileSegments = fileSegments;
            _compression = compression;
            _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing = sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing;
            Result = BuildObservable();

        }

        private IObservable<FileIndexCollection> BuildObservable()
        {
            return _fileSegments.Publish(shared =>
            {
                //Invoked at roll-over or file cleared
                var newFileCreated = shared
                    .Select(fsr => fsr.Changes.Reason)
                    .DistinctUntilChanged()
                    .Where(reason => reason == FileNotificationReason.CreatedOrOpened)
                    .Skip(1);

                //return empty when file does not exists
                var whenEmpty = shared
                    .Where(fsr => !fsr.Changes.Exists)
                    .Select(_ => FileIndexCollection.Empty)
                    .DistinctUntilChanged();

                var indexedFiles = BuildIndicies(shared)
                    .TakeUntil(newFileCreated)
                    .Repeat();
                
                return indexedFiles.Merge(whenEmpty).DistinctUntilChanged();
            });
        }
        private IObservable<FileIndexCollection> BuildIndicies(IObservable<FileSegmentReport> shared)
        {
            return Observable.Create<FileIndexCollection>(observer =>
            {
                //1. create  a resulting index object from the collection of index fragments
                var indexList = new SourceCache<Index, IndexType>(idx => idx.Type);


                //2. From those indicies, combine and build a new collection
                var collectionBuilder = indexList.Connect()
                    .Sort(SortExpressionComparer<Index>.Ascending(si => si.Start))
                    .ToCollection()
                    .CombineLatest(shared, (collection, report) => new { Collection = collection, Report = report })
                    .Scan((FileIndexCollection)null, (previous, x) =>
                    {
                        var tail = x.Report.TailInfo;
                        var changes = x.Report.Changes;
                        var segments = x.Report.Segments;
                        if (segments.Count == 0 || !changes.Exists)
                        {
                            return new FileIndexCollection(x.Collection, null, changes, tail);
                        }
                        return new FileIndexCollection(x.Collection, previous, changes, tail);
                    })
                    //.StartWith(FileIndexCollection.Empty)
                    .DistinctUntilChanged()
                    .SubscribeSafe(observer);


                var indexer = CreateIndicies(shared)
                    .Subscribe(index => indexList.AddOrUpdate(index));

                return new CompositeDisposable(indexer,  collectionBuilder, indexList);
            });
        }

        private IObservable<Index> CreateIndicies(IObservable<FileSegmentReport> shared)
        {
            //Do initial scan of tail, 
            //then index rest of the file + continue to index the tail 
            var tailScanner = shared
                .Select(report => report.TailInfo)
                .DistinctUntilChanged()
                .Scan((Index) null, (previous, tail) =>
                {
                    //index the tail
                    var indicies = tail.Lines.Select(l => l.LineInfo.Start).ToArray();
                    var index = new Index(tail.Start, tail.End, indicies, 1, tail.Count, IndexType.Tail);
                    return previous == null ? index : new Index(index, previous);
                });

            var estimater = shared
                .Select(report =>
                {
                    var estimatedLines = EstimateNumberOfLines(report);
                    return new Index(0, report.TailInfo.Start, _compression, estimatedLines, IndexType.Page);
                }).Take(1);

            var headIndexer = shared
                .FirstAsync()
                .Where(segments => segments.TailInfo.Start < _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing)
                .Take(1)
                .Select(segments =>
                {
                    var tail = segments.TailInfo;
                    var segment = segments.Segments;
                    var result = Scan(segment.Metrics, 0, tail.Start, _compression);
                    return result;
                })
                .Where(result=>result!=null);

            var indexer = estimater.Merge(headIndexer);

            return tailScanner.Merge(indexer);
        }


        private int EstimateNumberOfLines(FileSegmentReport fileSegmentsWithTail)
        {
            var tail = fileSegmentsWithTail.TailInfo;
            var segment = fileSegmentsWithTail.Segments;

            if (tail.Count == 0)
                return 0;

            //TODO: Need to account for line delimiter
            //var delimiter = fileSegmentsWithTail.Segments.Info.FindDelimiter();
            //var delimiterLength = Math.Max(2, delimiter);
            //Calculate estimate line count
            var averageLineLength = tail.Size / tail.Count;
            var estimatedLines = (segment.FileSize) / averageLineLength;
            return (int)estimatedLines;
        }

        private Index Scan(IFileMetrics metrics, long start, long end, int compression)
        {
            int count = 0;
            long lastPosition = 0;
            using (var stream = File.Open(metrics.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, metrics.Encoding, false))
                {
                    var currentPosition = reader.AbsolutePosition();
                    if (currentPosition != start)
                        stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream) return null;

                    lines = ScanLines(reader, compression, i => i, (line, position) =>
                    {
                        var shouldBreak = end != -1 && lastPosition >= end;
                        if (!shouldBreak)
                        {
                            //do not count the last line as this will take us one line over
                            lastPosition = position;
                            count++;
                        }
                        return shouldBreak;

                    }).ToArray();
                }

                if (end != -1 && lastPosition > end)
                {
                    count--;
                    lastPosition = end;
                    lines = lines.Take(lines.Length - 1).ToArray();
                }
                return new Index(start, lastPosition, lines, compression, count, end == -1 ? IndexType.Tail : IndexType.Page);
            }
        }

        private static IEnumerable<T> ScanLines<T>(StreamReaderExtended source,
            int compression,
            Func<long, T> selector,
            Func<string, long, bool> shouldBreak)
        {

            int i = 0;
            if (source.EndOfStream) yield break;

            string line;
            while ((line = source.ReadLine()) != null)
            {
                i++;
                var position = source.AbsolutePosition();

                if (shouldBreak(line, position))
                    yield break;

                if (i == compression)
                {
                    yield return selector(position);
                    i = 0;
                }
            }
        }
    }
}