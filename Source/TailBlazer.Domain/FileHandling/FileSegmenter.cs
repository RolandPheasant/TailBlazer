using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;

namespace TailBlazer.Domain.FileHandling
{
    /*
        Dynamically split the file into manageable chunks.

        Should resize as files grow [Not implemented yet]

        This is very useful for 
        i) partitioned searching
        ii) specific monitoring of the head
        iii) fast loading of initial file
    */

    public sealed class FileSegmenter
    {
       // private  FileInfo _info;
        private readonly int _initialTail;
        private readonly int _segmentSize;

        public IObservable<FileSegmentCollection> Segments { get; }

        public FileSegmenter(IObservable<FileNotification> notifications, 
                                int initialTail= 10000, 
                                int segmentSize=25000000)
            :this(notifications.MonitorChanges(), initialTail, segmentSize)
        {
            }


        public FileSegmenter(IObservable<FileChanges> notifications,
                        int initialTail = 500000,
                        int segmentSize = 25000000)
        {
            if (notifications == null) throw new ArgumentNullException(nameof(notifications));
            _initialTail = initialTail;
            _segmentSize = segmentSize;



            Segments = notifications.Publish(shared =>
            {
                //return empty when file does not exists
                var whenEmpty = shared
                    .Where(fc => !fc.ExistsAndIsValid())
                    .Select(fc => new FileSegmentCollection(fc));

                //
                var whenNotEmpty = shared
                        .Where(fc => fc.ExistsAndIsValid())
                        .Scan((FileSegmentCollection)null, (previous, current) =>
                        {
                            if (previous == null 
                                || current.Reason == FileNotificationReason.CreatedOrOpened
                                || previous.Reason == FileSegmentChangedReason.New)
                            {
                                var segments = LoadSegments(current.FullName).ToArray();
                                return new FileSegmentCollection(current, segments);
                            }
                            return new FileSegmentCollection(current, previous);
                        })
                        .DistinctUntilChanged();

                return whenEmpty.Merge(whenNotEmpty).DistinctUntilChanged();
            });


        }

        private IEnumerable<FileSegment> LoadSegments(string name)
        {
            using (var stream = File.Open(name, FileMode.Open, FileAccess.Read,FileShare.Delete | FileShare.ReadWrite))
            {
                var fileLength = stream.Length;

                stream.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReaderExtended(stream, true))
                {
                    if (reader.EndOfStream ||  fileLength == 0)
                    {
                        yield return new FileSegment(0, 0, 0, FileSegmentType.Tail);
                        yield break;
                    }

                    if (fileLength < _initialTail)
                    {
                        yield return new FileSegment(0, 0, fileLength, FileSegmentType.Tail);
                        yield break;
                    }

                    var headStartsAt = reader.FindNextEndOfLinePosition(fileLength - _initialTail);
                    long currentEnfOfPage = 0;
                    long previousEndOfPage = 0;

                    int index = 0;
                    do
                    {
                        var approximateEndOfPage = currentEnfOfPage + _segmentSize;
                        if (approximateEndOfPage >= headStartsAt)
                        {
                            yield return new FileSegment(index, previousEndOfPage, headStartsAt, FileSegmentType.Head);
                            break;
                        }
                        currentEnfOfPage = reader.FindNextEndOfLinePosition(approximateEndOfPage);
                        yield return new FileSegment(index, previousEndOfPage, currentEnfOfPage, FileSegmentType.Head);

                        index++;
                        previousEndOfPage = currentEnfOfPage;

                    } while (true);


                    index ++;
                    yield return new FileSegment(index, headStartsAt, fileLength, FileSegmentType.Tail);
                }
            }
        }

    }
}