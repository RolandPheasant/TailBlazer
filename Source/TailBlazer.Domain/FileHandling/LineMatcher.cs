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
        private readonly StreamReader _reader;

        public Encoding Encoding { get; }

        public int Lines => _index;

        private int _index = -1;

        public LineMatcher(FileInfo info, Func<string,bool> predicate, Encoding encoding = null)
        {
            _info = info;
            _predicate = predicate;
            Encoding = encoding ?? Encoding.Default;

            _stream = File.Open(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
            _reader = new StreamReader(_stream, Encoding, true);
        }

        public IEnumerable<int> ScanToEnd()
        {
            if (_reader.EndOfStream)
                yield break;

            string line;
            while ((line = _reader.ReadLine()) != null)
            {
                _index++;
                if (_predicate(line)) yield return _index;
            }
        }

        public void Dispose()
        {
            _stream.Dispose();
            _reader.Dispose();
        }
    }
}