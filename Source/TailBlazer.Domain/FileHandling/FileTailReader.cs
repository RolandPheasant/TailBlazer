using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class FileTailReader
    {
        private readonly IObservable<FileSegmentCollection> _fileSegmentCollection;

        public FileTailReader([NotNull] IObservable<FileSegmentCollection> fileSegmentCollection)
        {
            _fileSegmentCollection = fileSegmentCollection;
        }
        
        public IObservable<TailInfo> Tail()
        {
            //1. Load tail of an initial size when stream starts
            //2. Load subsequent tail as changes are made

            //continually return a changing tail 
            return Observable.Create<TailInfo>(observer =>
            {
                var lastTail = TailInfo.Empty;
                var locker = new object();

                return _fileSegmentCollection
                //    .Where(fsc=> fsc.Reason== FileSegmentChangedReason.Tailed)
                    .Synchronize(locker)
                    .Select(fsc =>
                    {
                        if (fsc.Count == 0 || fsc.SizeDiff < 0)
                            return TailInfo.Empty;

                        var tailFromPosition = fsc.Reason == FileSegmentChangedReason.Loaded 
                                ? fsc.Tail.Start
                                : lastTail.End;
                        
                        //calculate last tail => either inital size or scan from end
                        var lines = ReadTail(fsc.Metrics.FullName, tailFromPosition, fsc.Metrics.Encoding).ToArray();
                        var tail = new TailInfo(lines);
                        lastTail = tail;
                        return tail;
                    })
                    //.Where(info => info.Count != 0)
                    .SubscribeSafe(observer);
            });
        }
        
        private IEnumerable<Line> ReadTail(string fileName, long position, Encoding encoding)
        {
            int i = 0;
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, encoding, false))
                {
                    reader.BaseStream.Seek(position, SeekOrigin.Begin);

                    var startPosition = reader.AbsolutePosition();
                    do
                    {
                        var line = reader.ReadLine();
                        if (line == null) yield break;
                        
                        var endPosition = reader.AbsolutePosition();
                        var info = new LineInfo(i + 1, i, startPosition, endPosition);
                        yield return new Line(info, line, DateTime.UtcNow);

                        startPosition = endPosition;
                        i++;
                    } while (!reader.EndOfStream);
                }
            }
        }
    }

}