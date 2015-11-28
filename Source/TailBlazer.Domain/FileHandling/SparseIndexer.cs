using System;
using System.IO;
using System.Linq;
using System.Text;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{
    public class SparseIndexer
    {
        public IObservable<SparseIndex> IndexChanged { get; }
        public Encoding Encoding { get; }
        public FileInfo Info { get; }
        public int TailSize { get;  }


        public SparseIndexer([NotNull] FileInfo info,int tailSize=100000, Encoding encoding = null)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));

            Info = info;
            TailSize = tailSize;
            Encoding = encoding ?? info.GetEncoding();

            //1. Get  full length of file

            var indexer = new SparseIndexer(info);
            var start = (int)Math.Max(0, info.Length - tailSize);
            var end = (int)info.Length;


            //2. Scan tail (say 1 meg)

            //3. Can tail (say 1 meg)
            //report data back with an estimate of the number of Indicies

            //4. Continue with indexing whilst feedig back progress

            //take last MG for head


        }


        //public IObservable<SparseIndex> Run()
        //{
        //    return IDisposable


        //}

        public SparseIndex ScanToEnd(int start, int compression)
        {
            return Scan(start,-1, compression);
        }

        public SparseIndex Scan(int start, int end, int compression)
        {
            int count = 0;
            int lastPosition = 0;
            using (var stream = File.Open(Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                int[] lines;
                using (var reader = new StreamReaderExtended(stream, Encoding, false))
                {
                    stream.Seek(start, SeekOrigin.Begin);

                    lines = reader.ScanLines(compression, i => i, (line, position) =>
                    {
                        lastPosition = position;
                        count++;

                        return end!=-1 && lastPosition >= end;

                    }).ToArray();
                }
                
                if (lastPosition > end)
                {
                    //we have an overlapping line [must remove the last one from the head]
                }
                return new SparseIndex(start, lastPosition, lines, compression, count);
            }
        }
    }
}