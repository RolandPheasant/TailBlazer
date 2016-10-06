using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using DynamicData;
using DynamicData.Binding;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class Indexer
    {
        private readonly IObservable<FileSegmentsWithTail> _fileSegments;
        private readonly int _compression;
        private readonly int _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing;
        private readonly IScheduler _scheduler;

        public IObservable<FileIndexCollection> Result { get; }

        public Indexer([NotNull] IObservable<FileSegmentsWithTail> fileSegments,
            int compression = 10,
            int sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing = 250000000,
            IScheduler scheduler = null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));
            _fileSegments = fileSegments;
            _compression = compression;
            _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing = sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing;
            _scheduler = scheduler ?? Scheduler.Default;
            Result = BuildObservable();

        }

        private IObservable<FileIndexCollection> BuildObservable()
        {
            return Observable.Create<FileIndexCollection>(observer =>
            {
                var shared = _fileSegments.Publish();

                //0.5 ensure we have the latest file info and encoding
                Encoding encoding = null;
                FileInfo fileInfo = null;

                //TODO: This is shit. We need a better way of passing around the Encoding / File meta data
                var infoSubscriber = shared
                    .Where(fs => fs.Segments.Encoding != null)
                    .Take(1)
                    .Subscribe(fswt =>
                    {
                        encoding = fswt.Segments.Encoding;
                        fileInfo = fswt.Segments.Info;
                    });


                var tailWatcher = shared
                    .Select(segments => segments.TailInfo)
                    .DistinctUntilChanged()
                    .Publish();

                //1. create  a resulting index object from the collection of index fragments
                var indexList = new SourceList<Index>();

                var collectionBuilder = indexList.Connect()
                    .Sort(SortExpressionComparer<Index>.Ascending(si => si.Start))
                    .ToCollection()
                    .CombineLatest(tailWatcher, (collection, tail) => new { Collection = collection, Tail = tail })
                    .Scan((FileIndexCollection)null, (previous, x) => new FileIndexCollection(x.Collection,  previous, fileInfo, encoding))
                    .StartWith(FileIndexCollection.Empty)
                    .SubscribeSafe(observer);


                //2. continual indexing of the tail + replace tail index whenether there are new scan results
                var tailScanner = tailWatcher
                    .Scan((Index)null, (previous, tail) =>
                    {
                        //index the tail
                        var indicies = tail.Lines.Select(l => l.LineInfo.Start).ToArray();
                        var index = new Index(tail.Start, tail.End, indicies, 1, tail.Count, IndexType.Tail);
                        return previous == null ? index : new Index(index, previous);
                    }).Subscribe(tail =>
                    {
                        //write tail  
                        indexList.Edit(innerList =>
                        {
                            var existing = innerList.FirstOrDefault(si => si.Type == IndexType.Tail);
                            if (existing != null) innerList.Remove(existing);
                            innerList.Add(tail);
                        });
                    });


                //3. Index the remainer of the file, only after the head has been indexed
                var headSubscriber = shared
                    .FirstAsync()
                    .ObserveOn(_scheduler)
                    .Subscribe(segments =>
                    {
                        var tail = segments.TailInfo;
                        var segment = segments.Segments;

                        if (tail.Start == 0) return;

                        //add an estimate for the file size
                        var estimateLines = EstimateNumberOfLines(segments);
                        var estimate = new Index(0, tail.Start, _compression, estimateLines, IndexType.Page);
                        indexList.Add(estimate);

                        if (tail.Start < _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing)
                        {
                            //keep it as an estimate for files over 250 meg [for now]
                            //Perhaps we could correctly use segments an index entire file
                            //todo: index first and last segment for large sized file
                            //Produce indicies 
                            var actual = Scan(segment.Info.FullName, segment.Encoding, 0, tail.Start, _compression);
                            indexList.Edit(innerList =>
                            {
                                innerList.Remove(estimate);
                                innerList.Add(actual);
                            });
                        }
                    });
                return new CompositeDisposable(tailWatcher.Connect(), shared.Connect(), collectionBuilder, tailScanner, indexList, headSubscriber, infoSubscriber);
            });
        }


        private int EstimateNumberOfLines(FileSegmentsWithTail fileSegmentsWithTail)
        {
            var tail = fileSegmentsWithTail.TailInfo;
            var segment = fileSegmentsWithTail.Segments;

            //TODO: Need to account for line delimiter
            //var delimiter = fileSegmentsWithTail.Segments.Info.FindDelimiter();
            //var delimiterLength = Math.Max(2, delimiter);
            //Calculate estimate line count
            var averageLineLength = tail.Size / tail.Count;
            var estimatedLines = (segment.TailStartsAt) / averageLineLength;
            return (int)estimatedLines;
        }

        private Index Scan(string fileName, Encoding encoding, long start, long end, int compression)
        {
            int count = 0;
            long lastPosition = 0;
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, encoding, false))
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

    public class FileIndexer
    {
        private readonly IObservable<FileSegmentsWithTail> _fileSegments;
        private readonly int _compression;
        private readonly int _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing;
        private readonly IScheduler _scheduler;

        public IObservable<IndexCollection> Result { get; }

        public FileIndexer([NotNull] IObservable<FileSegmentsWithTail> fileSegments,
            int compression = 10,
            int sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing= 250000000,
            IScheduler scheduler = null)
        {
            if (fileSegments == null) throw new ArgumentNullException(nameof(fileSegments));
            _fileSegments = fileSegments;
            _compression = compression;
            _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing = sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing;
            _scheduler = scheduler ?? Scheduler.Default;
            Result = BuildObservable();

        }
        
        private IObservable<IndexCollection> BuildObservable()
        {
            return Observable.Create<IndexCollection>(observer =>
            {
                var shared = _fileSegments.Publish();

                //0.5 ensure we have the latest file info and encoding
                Encoding encoding = null;
                FileInfo fileInfo = null;

                //TODO: This is shit. We need a better way of passing around the Encoding / File meta data
                var infoSubscriber = shared
                    .Where(fs => fs.Segments.Encoding != null)
                    .Take(1)
                    .Subscribe(fswt =>
                    {
                        encoding = fswt.Segments.Encoding;
                        fileInfo = fswt.Segments.Info;
                    });
                

                var tailWatcher = shared
                    .Select(segments => segments.TailInfo)
                    .DistinctUntilChanged()
                    .Publish();

                //1. create  a resulting index object from the collection of index fragments
                var indexList = new SourceList<Index>();

                var collectionBuilder = indexList.Connect()
                    .Sort(SortExpressionComparer<Index>.Ascending(si => si.Start))
                    .ToCollection()
                    .CombineLatest(tailWatcher, (collection, tail) => new { Collection = collection, Tail = tail})
                    .Scan((IndexCollection) null, (previous, x) => new IndexCollection(x.Collection,x.Tail, previous, fileInfo, encoding))
                    .StartWith(IndexCollection.Empty)
                    .SubscribeSafe(observer);
                    

                //2. continual indexing of the tail + replace tail index whenether there are new scan results
                var tailScanner = tailWatcher
                    .Scan((Index) null, (previous, tail) =>
                    {
                        //index the tail
                        var indicies = tail.Lines.Select(l => l.LineInfo.Start).ToArray();
                        var index = new Index(tail.Start, tail.End, indicies, 1, tail.Count, IndexType.Tail);
                        return previous == null ? index : new Index(index, previous);
                    }).Subscribe(tail =>
                    {
                        //write tail  
                        indexList.Edit(innerList =>
                        {
                            var existing = innerList.FirstOrDefault(si => si.Type == IndexType.Tail);
                            if (existing != null) innerList.Remove(existing);
                            innerList.Add(tail);
                        });
                    });


                //3. Index the remainer of the file, only after the head has been indexed
                var headSubscriber = shared
                    .FirstAsync()
                    .ObserveOn(_scheduler)
                    .Subscribe(segments =>
                    {
                        var tail = segments.TailInfo;
                        var segment = segments.Segments;

                        if (tail.Start == 0) return;
                        
                        //add an estimate for the file size
                        var estimateLines = EstimateNumberOfLines(segments);
                        var estimate = new Index(0, tail.Start, _compression, estimateLines, IndexType.Page);
                        indexList.Add(estimate);

                        if (tail.Start < _sizeOfFileAtWhichThereIsAbsolutelyNoPointInIndexing)
                        {
                            //keep it as an estimate for files over 250 meg [for now]
                            //Perhaps we could correctly use segments an index entire file
                            //todo: index first and last segment for large sized file
                            //Produce indicies 
                            var actual = Scan(segment.Info.FullName, segment.Encoding, 0, tail.Start, _compression);
                            indexList.Edit(innerList =>
                            {
                                innerList.Remove(estimate);
                                innerList.Add(actual);
                            });
                        }
                    });
                return new CompositeDisposable(tailWatcher.Connect(), shared.Connect(), collectionBuilder, tailScanner, indexList, headSubscriber, infoSubscriber);
            });
        }


        private int EstimateNumberOfLines(FileSegmentsWithTail fileSegmentsWithTail)
        {
            var tail = fileSegmentsWithTail.TailInfo;
            var segment = fileSegmentsWithTail.Segments;

            //TODO: Need to account for line delimiter
            //var delimiter = fileSegmentsWithTail.Segments.Info.FindDelimiter();
            //var delimiterLength = Math.Max(2, delimiter);
            //Calculate estimate line count
            var averageLineLength = tail.Size / tail.Count;
            var estimatedLines = (segment.TailStartsAt) / averageLineLength;
            return (int)estimatedLines;
        }

        private Index Scan(string fileName, Encoding encoding, long start, long end, int compression)
        {
            int count = 0;
            long lastPosition = 0;
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                long[] lines;
                using (var reader = new StreamReaderExtended(stream, encoding, false))
                {
                    var currentPosition = reader.AbsolutePosition();
                    if (currentPosition!= start)
                        stream.Seek(start, SeekOrigin.Begin);
                    if (reader.EndOfStream) return null;

                    lines = ScanLines(reader,compression, i => i, (line, position) =>
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

                if (end != -1 && lastPosition> end)
                {
                    count--;
                    lastPosition = end;
                    lines = lines.Take(lines.Length - 1).ToArray();
                }

                return new Index(start, lastPosition, lines, compression, count, end == -1 ? IndexType.Tail : IndexType.Page);
            }
        }

        private  static IEnumerable<T> ScanLines<T>( StreamReaderExtended source,
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