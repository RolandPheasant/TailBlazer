using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class LineMatcher: IDisposable
    {
        private readonly FileInfo _info;
        private readonly Func<string, bool> _predicate;
        private readonly FileStream _stream;
        private readonly StreamReaderExtended _reader;

        public Encoding Encoding { get; }
        public int TotalCount => _index;
        public int Count => _matches;

        private int _index = -1;
        private int _matches = -1;

        public LineMatcher(FileInfo info, Func<string,bool> predicate, Encoding encoding = null)
        {
            _info = info;
            _predicate = predicate;
            Encoding = encoding ?? Encoding.Default;

            _stream = File.Open(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
            _reader = new StreamReaderExtended(_stream, Encoding, true);

            //TODO 1: Get current encoding from _reader.CurrentEncoding and expose so cusumers can read the lines with the same encoding
            //TODO 2: Expose line delimiter length so it can be correctly removed from when re-reading a line
        }

        public IEnumerable<int> ScanToEnd()
        {
            if (_reader.EndOfStream)
                yield break;

            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                _index++;
                if (!_predicate(line)) continue;
                _matches++;
                yield return _index;
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
            _reader.Dispose();
        }
    }
}