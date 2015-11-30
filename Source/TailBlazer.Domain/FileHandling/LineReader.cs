using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.FileHandling
{

    public static class LineReader
    {
        public static IEnumerable<T> ScanLines<T>(this StreamReaderExtended source,
                int compression,
                Func<int, T> selector,
                Func<string, int,bool> shouldBreak)
        {

            int i = 0;

        

            if (source.EndOfStream) yield break;

            string line;
            while ((line=source.ReadLine()) != null)
            {
                i++;
                var position = (int)source.AbsolutePosition();

                if (i == compression)
                {
                    yield return selector(position);
                    i = 0;
                };
                
                if (shouldBreak(line, position))
                    yield break;
            }

        }

        
        public static IEnumerable<T> ReadLine<T>(this FileInfo source, IEnumerable<LineIndex> lines, Func<LineIndex, string, T> selector, Encoding encoding)
        {
            encoding = encoding ?? source.GetEncoding();

      
            var indicies = lines.OrderBy(l => l.Index).ToArray();

            if (indicies.Length==0) yield break;

            var first = indicies[0];
            var relative = first.Type == LineIndexType.Relative;

            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                stream.Seek(0, SeekOrigin.Begin);
          
                using (var reader = new StreamReaderExtended(stream, encoding,false))
                {

                    if (relative)
                    {
                        int previousLine = -1;
                        long previousStart = -1;

                        foreach (var index in indicies)
                        {
                            var currentLine = index.Line;
                            var currentStart = index.Start;

                            var isContinuous = (currentLine == previousLine + 1 && currentStart==previousStart);
                            if (!isContinuous)
                            {
                                reader.DiscardBufferedData();
                                stream.Seek(index.Start, SeekOrigin.Begin);

                                if (index.Offset > 0)
                                {
                                    //skip number of lines offset
                                    for (int i = 0; i < index.Offset; i++)
                                        reader.ReadLine();
                                }

                            }

                            var line = reader.ReadLine();
                            yield return selector(index, line);

                            previousLine = currentLine;
                            previousStart = currentStart;
                        }

                    }
                    else
                    {
                        int previousLine = -1;
                        foreach (var index in indicies)
                        {
                            var currentLine = index.Line;
                            var isContinuous = currentLine == previousLine + 1;
                            if (!isContinuous)
                            {
                                reader.DiscardBufferedData();
                                reader.BaseStream.Seek(index.Start, SeekOrigin.Begin);
                            }

                            var line = reader.ReadLine();
                            yield return selector(index, line);
                            previousLine = currentLine;
                        }
                    }

                }
            }
        }
        
        /// <summary>
        /// Finds the delimiter by looking for the first line in the file and comparing chars
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static int  FindDelimiter(this FileInfo source)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, Encoding.Default, true))
                {
                    if (reader.EndOfStream)
                        return -1;
                    do
                    {
                        var ch = (char)reader.Read();

                        // Note the following common line feed chars: 
                        // \n - UNIX   \r\n - DOS   \r - Mac 
                        switch (ch)
                        {
                            case '\r':
                                var next = (char)reader.Peek();
                                //with \n is WINDOWS delimiter. Otherwise mac
                                return next == '\n' ? 2 : 1;
                            case '\n':
                                return 1;
                        }
                    } while (!reader.EndOfStream);
                    return -1;
                }
            }
        }

        /// <summary>
        /// Determines the encoding of a file
        /// </summary>
        /// <returns></returns>
        public static Encoding GetEncoding(this FileInfo source)
        {
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, true))
                {
                    var something = reader.Peek();
                    return reader.CurrentEncoding;
                }
            }
        }
    }
}