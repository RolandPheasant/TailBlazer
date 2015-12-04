using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class LineIndexer : IDisposable
    {
        private readonly FileInfo _info;
        private readonly FileStream _stream;
        private readonly StreamReaderExtended _reader;

        public  Encoding Encoding { get; }

        public long Positon => _postion;
        public int Lines => _index;

        private long _postion;
        private int _index = -1;

        public LineIndexer(FileInfo info)
        {
            _info = info;

            Encoding = info.GetEncoding();
            _stream = File.Open(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
            _reader = new StreamReaderExtended( _stream, Encoding, false);
        }

        public IEnumerable<long> ReadToEnd()
        {
            if (_reader.EndOfStream) yield break;
            
            while ((_reader.ReadLine()) != null)
            {
                _index++;
                _postion = _reader.AbsolutePosition();
                yield return _postion;
            }
        }
        public void Dispose()
        {
            _stream.Dispose();
            _reader.Dispose();
        }
    }

}
