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
        private readonly StreamReader _reader;

        public  Encoding Encoding { get; }
        public int Positon => _postion;
        public int Lines => _index;

        private int _postion;
        private int _index = -1;

        private int _delimiterSize =-1;

        public LineIndexer(FileInfo info, Encoding encoding = null)
        {
            _info = info;
            Encoding = encoding ?? Encoding.Default;

            _stream = File.Open(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
            _reader = new StreamReader(_stream, Encoding, true);
        }

        public IEnumerable<int> ReadToEnd()
        {
            string line;

            if (_reader.EndOfStream)
                yield break;

            if (_delimiterSize==-1 )
            {
                _delimiterSize = _info.FindDelimiter();
                if (_delimiterSize==-1)
                    throw new FileLoadException("Cannot determine new line delimiter");
            }

            while ((line = _reader.ReadLine()) != null)
            {
                _index++;
                _postion = _postion + line.Length + _delimiterSize;
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
