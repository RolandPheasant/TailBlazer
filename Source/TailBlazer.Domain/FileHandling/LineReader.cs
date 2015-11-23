using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public static class LineReader
    {
        public static LineIndex InferPosition(this int[] source, int index)
        {
            var current = source[index];

            var previous=0;
            if (index != 0)
            {
                previous = source[index - 1];
            }
            return new LineIndex(index+1, index, (long)previous, (long)current);
        }


        public static IEnumerable<T> ReadLine<T>(this FileInfo source, IEnumerable<LineIndex> lines, Func<LineIndex, string, T> selector)
        {
            var indicies = lines.OrderBy(l => l.Index);
            using (var stream = File.Open(source.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                // fast forward to our starting point
                foreach (var line in indicies)
                {
                    var content = new byte[line.Size-2]; //remove line ending (we need to get this scientifically)
                    stream.Seek(line.Start, SeekOrigin.Begin);
                    stream.Read(content, 0, content.Length);

                    var str = Encoding.UTF8.GetString(content);
                    yield return selector(line, str);
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
                using (var reader = new StreamReader(stream, Encoding.Default, true))
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
                using (var reader = new StreamReader(stream, true))
                {
                    var something = reader.Peek();
                    return reader.CurrentEncoding;
                }
            }
        }
    }
}