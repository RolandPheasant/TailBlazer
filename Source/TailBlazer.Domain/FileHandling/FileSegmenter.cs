using System;
using System.Drawing.Printing;
using System.Reactive.Disposables;
using DynamicData;

namespace TailBlazer.Domain.FileHandling
{

    public enum FileSegmentType
    {
        Head,
        Body,
        Tail
    }

    public class FileSegment
    {
        public int Index { get; }
        public int Start { get;  }
        public int End { get;  }
        public FileSegmentType Type { get;  }

        public FileSegment(int index, int start, int end, FileSegmentType type)
        {
            Index = index;
            Start = start;
            End = end;
            Type = type;
        }
    }

    public sealed class FileSegmenter: IDisposable
    {
        //dynamically split the file into segments according to the size of file.
        //as the file size changes, allow these segments to dynamically resize

       //additionally seperately  monitor the head of the file??
       
       //this is very useful for parallelising searches searches.
       private readonly IObservableCache<FileSegment,int> _cache = new SourceCache<FileSegment, int>(fs=>fs.Index);

        private IDisposable _cleanup;

        public FileSegmenter(int endSize= 1000000, int resizeFactor=10)
        {
            //calculate 

            _cleanup = new CompositeDisposable(_cache);
        }

        public void Dispose()
        {

        }
    }
}