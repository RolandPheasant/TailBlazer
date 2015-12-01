using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.FileHandling
{
    /*
        Dynamically split the file into manageable chunks.

        Should resize as files grow [Not implemented yet]

        This is very useful for 
        i) partitioned searching
        ii) specific moonitoring of the head
        iii) fast loading of initial file
    */

    public sealed class FileSegmenter
    {
        private readonly FileInfo _info;
        private readonly int _initialTail;
        private readonly int _segmentSize;

        public IObservable<FileSegments> Segments { get; }

        public FileSegmenter(FileInfo info,
                                IObservable<Unit> refresher, 
                                int initialTail= 100000, 
                                int segmentSize=25000000)
        {
            if (refresher == null) throw new ArgumentNullException(nameof(refresher));
            _info = info;
            _initialTail = initialTail;
            _segmentSize = segmentSize;
 
            Segments = refresher
                //.StartWithUnit()
                .Scan((FileSegments) null, (previous, current) =>
                {
                    if (previous==null)
                        return new FileSegments(LoadSegments().ToArray());

                    var newLength = info.GetFileLength();
                    return new FileSegments(newLength, previous);
                }).Replay(1).RefCount();
        }

        public IEnumerable<FileSegment> LoadSegments()
        {
            var fileLength = _info.Length;
            if (fileLength == 0)
            {
                yield return new FileSegment(0,0,0, FileSegmentType.Tail); 
                yield break;
            }

            if (fileLength < _initialTail)
            {
                yield return new FileSegment(0, 0, fileLength, FileSegmentType.Tail);
                yield break;
            }

            var headStartsAt = _info.FindNextEndOfLinePosition(fileLength -_initialTail);
            long currentEnfOfPage = 0;
            long previousEndOfPage = 0;

            int index = 0;
            do
            {
                var approximateEndOfPage = currentEnfOfPage + _segmentSize;
                if (approximateEndOfPage >= headStartsAt)
                {
                    yield return new FileSegment(index, previousEndOfPage,headStartsAt,FileSegmentType.Head);
                    break;
                }
                currentEnfOfPage = _info.FindNextEndOfLinePosition(approximateEndOfPage);
                yield return new FileSegment(index, previousEndOfPage, currentEnfOfPage, FileSegmentType.Head);

                index++;
                previousEndOfPage = currentEnfOfPage;

            } while (true);


            index ++;
            yield return new FileSegment(index, headStartsAt, fileLength, FileSegmentType.Tail);
        }

    }
}