using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{
    public sealed class FileSegmenter: IDisposable
    {
        private readonly FileInfo _info;
        private readonly int _initialTail;
        private readonly int _tailSize;
        private readonly int _segmentSize;
        //dynamically split the file into segments according to the size of file.
        //as the file size changes, allow these segments to dynamically resize

       //additionally seperately  monitor the head of the file??
       
       //this is very useful for parallelising searches searches.
       private readonly IObservableCache<FileSegment,int> _cache = new SourceCache<FileSegment, int>(fs=>fs.Index);

        private IDisposable _cleanup;

        public FileSegmenter(FileInfo info,
                                int initialTail= 10000, 
                                int tailSize = 10000000,
                                int segmentSize=25000000)
        {
            _info = info;
            _initialTail = initialTail;
            _tailSize = tailSize;
            _segmentSize = segmentSize;
            //calculate 

            _cleanup = new CompositeDisposable(_cache);
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

            var headStartsAt = _info.FindNextEndOfLinePosition(_initialTail);


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
            yield return new FileSegment(index, headStartsAt, fileLength, FileSegmentType.Head);
        }

        public void Dispose()
        {

        }
    }
}