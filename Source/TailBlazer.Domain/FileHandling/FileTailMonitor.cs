using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class FileTailReader
    {
        private readonly IObservable<FileSegmentCollection> _fileSegmentCollection;
        
        public FileTailReader([NotNull] IObservable<FileSegmentCollection> fileSegmentCollection)
        {
            _fileSegmentCollection = fileSegmentCollection;
        }

        public IObservable<FileTailInfo> Tail()
        {
            //continually return a changin tail 
            return Observable.Create<FileTailInfo>(observer =>
            {
                Encoding encoding = null;

                return _fileSegmentCollection
                    .Select(fsc =>
                    {
                        if (fsc.Count == 0)
                            return FileTailInfo.Empty;

                        if (encoding == null)
                            encoding = fsc.Info.GetEncoding();

                        var lines = ReadLines(fsc, encoding);
                        return new FileTailInfo(lines.ToArray());
                    })
                    .Where(info => info.Count != 0)
                    .SubscribeSafe(observer);
            });
        }
        
        private IEnumerable<Line> ReadLines(FileSegmentCollection segments, Encoding encoding)
        {
            var tail = segments.Tail;
            var fileName = segments.Info.FullName;
            var position = segments.Reason == FileSegmentChangedReason.Loaded ? 0 : tail.Start;

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

                        i++;

                        var endPosition = reader.AbsolutePosition();

                        var info = new LineInfo(i + 1, i, startPosition, endPosition);
                        yield return new Line(info, line, DateTime.UtcNow);

                        startPosition = endPosition;

                    } while (!reader.EndOfStream);
                }
            }
        }
    }

}