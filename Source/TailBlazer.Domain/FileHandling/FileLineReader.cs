using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public class FileLineReaderWriter: IDisposable
    {
        private FileStream _stream;
        private readonly StreamReader _reader;
        private readonly Encoding _encoding;

        private readonly byte[] _newLine;
        private readonly int[] _buffer = new int[1024];
        private int _postion;
        private int _index=-1;

        //private readonly 

        public FileLineReaderWriter(FileInfo info, Encoding encoding=null)
        {

            _stream = File.Open(info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite);
            _stream.Seek(0, SeekOrigin.Begin);
            _reader = new StreamReader(_stream, Encoding.Default, true);
            _encoding = encoding ?? Encoding.Default;

            _newLine = _encoding.GetBytes(Environment.NewLine);

        }


        public IEnumerable<LineIndex> ReadToEnd()
        {
            do
            {
                yield return Next();

            } while (!_reader.EndOfStream);
        }

        public LineIndex Next()
        {
            var startPosition = _postion +1;
            var i = 0;

            //loop through until we find the end of file marker
            //TODO: Also check for unix ones

            do
            {
                var current = _reader.Read();
                _buffer[i] = current;


                bool isEol=false;

                if (i >= _newLine.Length - 1 && current == _newLine[_newLine.Length - 1])
                {
                    //check for end of file

                    for (int k = _newLine.Length-1; k >0; k--)
                    {
                        var inBuffer = _buffer[i - k];
                        var inDelimiter = _newLine[k];

                        if (inBuffer != inDelimiter)
                            break;
                    }

                    isEol = true;
                }
                
                if (isEol) break;
                i++;



            } while (!_reader.EndOfStream);


            //check if empty [if no return nothing??s]

            _index++;
            var endPosition = _postion+i;


            _postion = endPosition;

            var newContentSize = i;
            var newContent = new byte[newContentSize];


          //  Debug.WriteLine($"This chunk will be {newContent.Length} bytes.");

            // fast forward to our starting point
          //  fs.Seek(startAt, SeekOrigin.Begin);

            // read the new data
         //   fs.Read(newContent, 0, newContent.Length);

            // detect new lines before attempting to update
            // if there aren't any, treat it as if the file is untouched
            var newContentString = Encoding.UTF8.GetString(newContent);

            return new LineIndex(_index, _index, startPosition, _postion);
        }

        


        public void Dispose()
        {
            _stream.Dispose();
            _reader.Dispose();
        }
    }
}
