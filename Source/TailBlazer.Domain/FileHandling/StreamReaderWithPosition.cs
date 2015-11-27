using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    // This class implements a TextReader for reading characters to a Stream. 
    // This is designed for character input in a particular Encoding,
    // whereas the Stream class is designed for byte input and output. 
    //

    /// <summary>
    /// Doctored from this
    /// 
    /// https://www.daniweb.com/programming/software-development/threads/35078/streamreader-and-position
    /// </summary>
    [Serializable()]
    public class StreamReaderWithPosition : TextReader
    {
        public new static readonly StreamReaderWithPosition Null = new NullStreamReader();
        internal const int DefaultBufferSize = 1024;  // Byte buffer size
        private const int DefaultFileStreamBufferSize = 4096;
        private const int MinBufferSize = 128;
        private Stream stream;
        private Encoding encoding;
        private Decoder decoder;
        private byte[] byteBuffer;
        private char[] charBuffer;
        private byte[] _preamble;
        private int charPos;
        private int charLen;
        private int byteLen;
        private int _maxCharsPerBuffer;
        private bool _detectEncoding;
        private bool _checkPreamble;
        private bool _isBlocked;
        private int _lineLength;
        public int LineLength => _lineLength;

        private int _bytesRead;
        public int BytesRead => _bytesRead;

        internal StreamReaderWithPosition()
        {
        }
        public StreamReaderWithPosition(Stream stream)
            : this(stream, Encoding.UTF8, true, DefaultBufferSize)
        {
        }
        public StreamReaderWithPosition(Stream stream, bool detectEncodingFromByteOrderMarks)
            : this(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize)
        {
        }
        public StreamReaderWithPosition(Stream stream, Encoding encoding)
            : this(stream, encoding, true, DefaultBufferSize)
        {
        }

        public StreamReaderWithPosition(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize = DefaultBufferSize)
        {
            if (stream == null || encoding == null) throw new ArgumentNullException((stream == null ? "stream" : "encoding"));
            if (!stream.CanRead) throw new ArgumentException(Environment.GetEnvironmentVariable("Argument_StreamNotReadable"));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize), Environment.GetEnvironmentVariable("ArgumentOutOfRange_NeedPosNum"));
            Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
        }

        public StreamReaderWithPosition(String path)
            : this(path, Encoding.UTF8, true, DefaultBufferSize)
        {
        }

        public StreamReaderWithPosition(String path, bool detectEncodingFromByteOrderMarks)
            : this(path, Encoding.UTF8, detectEncodingFromByteOrderMarks, DefaultBufferSize)
        {
        }

        public StreamReaderWithPosition(String path, Encoding encoding)
            : this(path, encoding, true, DefaultBufferSize)
        {
        }

        public StreamReaderWithPosition(String path, Encoding encoding, bool detectEncodingFromByteOrderMarks)
            : this(path, encoding, detectEncodingFromByteOrderMarks, DefaultBufferSize)
        {
        }

        public StreamReaderWithPosition(String path, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            // Don't open a Stream before checking for invalid arguments,
            // or we'll create a FileStream on disk and we won't close it until
            // the finalizer runs, causing problems for applications.
            if (path == null || encoding == null)
                throw new ArgumentNullException((path == null ? "path" : "encoding"));
            if (path.Length == 0)
                throw new ArgumentException(Environment.GetEnvironmentVariable("Argument_EmptyPath"));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException("bufferSize", Environment.GetEnvironmentVariable("ArgumentOutOfRange_NeedPosNum"));
            Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, DefaultFileStreamBufferSize);
            Init(stream, encoding, detectEncodingFromByteOrderMarks, bufferSize);
        }
        private void Init(Stream stream, Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize)
        {
            this.stream = stream;
            this.encoding = encoding;
            decoder = encoding.GetDecoder();
            if (bufferSize < MinBufferSize) bufferSize = MinBufferSize;
            byteBuffer = new byte[bufferSize];
            _maxCharsPerBuffer = encoding.GetMaxCharCount(bufferSize);
            charBuffer = new char[_maxCharsPerBuffer];
            byteLen = 0;
            _detectEncoding = detectEncodingFromByteOrderMarks;
            _preamble = encoding.GetPreamble();
            _checkPreamble = (_preamble.Length > 0);
            _isBlocked = false;
        }

        public override void Close()
        {
            Dispose(true);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (stream != null)
                    stream.Close();
            }
            if (stream != null)
            {
                stream = null;
                encoding = null;
                decoder = null;
                byteBuffer = null;
                charBuffer = null;
                charPos = 0;
                charLen = 0;
            }
            base.Dispose(disposing);
        }
        public virtual Encoding CurrentEncoding
        {
            get { return encoding; }
        }
        public virtual Stream BaseStream
        {
            get { return stream; }
        }
        // DiscardBufferedData tells StreamReaderWithPosition to throw away its internal
        // buffer contents.  This is useful if the user needs to seek on the
        // underlying stream to a known location then wants the StreamReaderWithPosition
        // to start reading from this new point.  This method should be called
        // very sparingly, if ever, since it can lead to very poor performance.
        // However, it may be the only way of handling some scenarios where 
        // users need to re-read the contents of a StreamReaderWithPosition a second time.
        public void DiscardBufferedData()
        {
            byteLen = 0;
            charLen = 0;
            charPos = 0;
            decoder = encoding.GetDecoder();
            _isBlocked = false;
        }
        public override int Peek()
        {
            //if (stream == null)
            //__Error.ReaderClosed();
            if (charPos == charLen)
            {
                if (_isBlocked || ReadBuffer() == 0) return -1;
            }
            return charBuffer[charPos];
        }
        public override int Read()
        {
            //if (stream == null)
            //__Error.ReaderClosed();
            if (charPos == charLen)
            {
                if (ReadBuffer() == 0) return -1;
            }
            return charBuffer[charPos++];
        }
        public override int Read([In, Out] char[] buffer, int index, int count)
        {
            //if (stream == null)
            //__Error.ReaderClosed();
            if (buffer == null)
                throw new ArgumentNullException("buffer", Environment.GetEnvironmentVariable("ArgumentNull_Buffer"));
            if (index < 0 || count < 0)
                throw new ArgumentOutOfRangeException((index < 0 ? "index" : "count"), Environment.GetEnvironmentVariable("ArgumentOutOfRange_NeedNonNegNum"));
            if (buffer.Length - index < count)
                throw new ArgumentException(Environment.GetEnvironmentVariable("Argument_InvalidOffLen"));
            int charsRead = 0;
            // As a perf optimization, if we had exactly one buffer's worth of 
            // data read in, let's try writing directly to the user's buffer.
            bool readToUserBuffer = false;
            while (count > 0)
            {
                int n = charLen - charPos;
                if (n == 0) n = ReadBuffer(buffer, index + charsRead, count, out readToUserBuffer);
                if (n == 0) break;  // We're at EOF
                if (n > count) n = count;
                if (!readToUserBuffer)
                {
                    Buffer.BlockCopy(charBuffer, charPos * 2, buffer, (index + charsRead) * 2, n * 2);
                    charPos += n;
                }
                charsRead += n;
                count -= n;
                // This function shouldn't block for an indefinite amount of time,
                // or reading from a network stream won't work right.  If we got
                // fewer bytes than we requested, then we want to break right here.
                if (_isBlocked)
                    break;
            }
            return charsRead;
        }

        public override String ReadToEnd()
        {
            //if (stream == null)
            //__Error.ReaderClosed();
            // For performance, call Read(char[], int, int) with a buffer
            // as big as the StreamReaderWithPosition's internal buffer, to get the 
            // readToUserBuffer optimization.
            char[] chars = new char[charBuffer.Length];
            int len;
            StringBuilder sb = new StringBuilder(charBuffer.Length);
            while ((len = Read(chars, 0, chars.Length)) != 0)
            {
                sb.Append(chars, 0, len);
            }
            return sb.ToString();
        }
        // Trims n bytes from the front of the buffer.
        private void CompressBuffer(int n)
        {
            Buffer.BlockCopy(byteBuffer, n, byteBuffer, 0, byteLen - n);
            byteLen -= n;
        }
        // returns whether the first array starts with the second array.
        private static bool BytesMatch(byte[] buffer, byte[] compareTo)
        {
            for (int i = 0; i < compareTo.Length; i++)
                if (buffer[i] != compareTo[i])
                    return false;
            return true;
        }
        private void DetectEncoding()
        {
            if (byteLen < 2)
                return;
            _detectEncoding = false;
            bool changedEncoding = false;
            if (byteBuffer[0] == 0xFE && byteBuffer[1] == 0xFF)
            {
                // Big Endian Unicode
                encoding = new UnicodeEncoding(true, true);
                decoder = encoding.GetDecoder();
                CompressBuffer(2);
                changedEncoding = true;
            }
            else if (byteBuffer[0] == 0xFF && byteBuffer[1] == 0xFE)
            {
                // Little Endian Unicode
                encoding = new UnicodeEncoding(false, true);
                decoder = encoding.GetDecoder();
                CompressBuffer(2);
                changedEncoding = true;
            }
            else if (byteLen >= 3 && byteBuffer[0] == 0xEF && byteBuffer[1] == 0xBB && byteBuffer[2] == 0xBF)
            {
                // UTF-8
                encoding = Encoding.UTF8;
                decoder = encoding.GetDecoder();
                CompressBuffer(3);
                changedEncoding = true;
            }
            else if (byteLen == 2)
                _detectEncoding = true;
            // Note: in the future, if we change this algorithm significantly,
            // we can support checking for the preamble of the given encoding.
            if (changedEncoding)
            {
                _maxCharsPerBuffer = encoding.GetMaxCharCount(byteBuffer.Length);
                charBuffer = new char[_maxCharsPerBuffer];
            }
        }
        private int ReadBuffer()
        {
            charLen = 0;
            byteLen = 0;
            charPos = 0;
            do
            {
                byteLen = stream.Read(byteBuffer, 0, byteBuffer.Length);
                if (byteLen == 0)  // We're at EOF
                    return charLen;
                // _isBlocked == whether we read fewer bytes than we asked for.
                // Note we must check it here because CompressBuffer or 
                // DetectEncoding will screw with byteLen.
                _isBlocked = (byteLen < byteBuffer.Length);
                if (_checkPreamble && byteLen >= _preamble.Length)
                {
                    _checkPreamble = false;
                    if (BytesMatch(byteBuffer, _preamble))
                    {
                        _detectEncoding = false;
                        CompressBuffer(_preamble.Length);
                    }
                }
                // If we're supposed to detect the encoding and haven't done so yet,
                // do it.  Note this may need to be called more than once.
                if (_detectEncoding && byteLen >= 2)
                    DetectEncoding();
                charLen += decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charLen);
            } while (charLen == 0);
            //Console.WriteLine("ReadBuffer called.  chars: "+charLen);
            return charLen;
        }
        // This version has a perf optimization to decode data DIRECTLY into the 
        // user's buffer, bypassing StreamWriter's own buffer.
        // This gives a > 20% perf improvement for our encodings across the board,
        // but only when asking for at least the number of characters that one
        // buffer's worth of bytes could produce.
        // This optimization, if run, will break SwitchEncoding, so we must not do 
        // this on the first call to ReadBuffer.  
        private int ReadBuffer(char[] userBuffer, int userOffset, int desiredChars, out bool readToUserBuffer)
        {
            charLen = 0;
            byteLen = 0;
            charPos = 0;
            int charsRead = 0;
            // As a perf optimization, we can decode characters DIRECTLY into a
            // user's char[].  We absolutely must not write more characters 
            // into the user's buffer than they asked for.  Calculating 
            // encoding.GetMaxCharCount(byteLen) each time is potentially very 
            // expensive - instead, cache the number of chars a full buffer's 
            // worth of data may produce.  Yes, this makes the perf optimization 
            // less aggressive, in that all reads that asked for fewer than AND 
            // returned fewer than _maxCharsPerBuffer chars won't get the user 
            // buffer optimization.  This affects reads where the end of the
            // Stream comes in the middle somewhere, and when you ask for 
            // fewer chars than than your buffer could produce.
            readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
            do
            {
                byteLen = stream.Read(byteBuffer, 0, byteBuffer.Length);
                if (byteLen == 0)  // EOF
                    return charsRead;
                // _isBlocked == whether we read fewer bytes than we asked for.
                // Note we must check it here because CompressBuffer or 
                // DetectEncoding will screw with byteLen.
                _isBlocked = (byteLen < byteBuffer.Length);
                // On the first call to ReadBuffer, if we're supposed to detect the encoding, do it.
                if (_detectEncoding && byteLen >= 2)
                {
                    DetectEncoding();
                    // DetectEncoding changes some buffer state.  Recompute this.
                    readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
                }
                if (_checkPreamble && byteLen >= _preamble.Length)
                {
                    _checkPreamble = false;
                    if (BytesMatch(byteBuffer, _preamble))
                    {
                        _detectEncoding = false;
                        CompressBuffer(_preamble.Length);
                        // CompressBuffer changes some buffer state.  Recompute this.
                        readToUserBuffer = desiredChars >= _maxCharsPerBuffer;
                    }
                }
                /*
				if (readToUserBuffer)
					Console.Write('.');
				else {
					Console.WriteLine("Desired chars is wrong.  byteBuffer.length: "+byteBuffer.Length+"  max chars is: "+encoding.GetMaxCharCount(byteLen)+"  desired: "+desiredChars);
				}
				*/
                charPos = 0;
                if (readToUserBuffer)
                {
                    charsRead += decoder.GetChars(byteBuffer, 0, byteLen, userBuffer, userOffset + charsRead);
                    charLen = 0;  // StreamReaderWithPosition's buffer is empty.
                }
                else
                {
                    charsRead = decoder.GetChars(byteBuffer, 0, byteLen, charBuffer, charsRead);
                    charLen += charsRead;  // Number of chars in StreamReaderWithPosition's buffer.
                }
            } while (charsRead == 0);
            //Console.WriteLine("ReadBuffer: charsRead: "+charsRead+"  readToUserBuffer: "+readToUserBuffer);
            return charsRead;
        }
        // Reads a line. A line is defined as a sequence of characters followed by
        // a carriage return ('\r'), a line feed ('\n'), or a carriage return
        // immediately followed by a line feed. The resulting string does not
        // contain the terminating carriage return and/or line feed. The returned
        // value is null if the end of the input stream has been reached.

        public override String ReadLine()
        {
            _lineLength = 0;
            //if (stream == null)
            //	__Error.ReaderClosed();
            if (charPos == charLen)
            {
                if (ReadBuffer() == 0) return null;
            }
            StringBuilder sb = null;
            do
            {
                int i = charPos;
                do
                {
                    char ch = charBuffer[i];
                    int EolChars = 0;
                    if (ch == '\r' || ch == '\n')
                    {
                        EolChars = 1;
                        string s;
                        if (sb != null)
                        {
                            sb.Append(charBuffer, charPos, i - charPos);
                            s = sb.ToString();
                        }
                        else
                        {
                            s = new string(charBuffer, charPos, i - charPos);
                        }
                        charPos = i + 1;
                        if (ch == '\r' && (charPos < charLen || ReadBuffer() > 0))
                        {
                            if (charBuffer[charPos] == '\n')
                            {
                                charPos++;
                                EolChars = 2;
                            }
                        }
                        _lineLength = s.Length + EolChars;
                        _bytesRead = _bytesRead + _lineLength;
                        return s;
                    }
                    i++;
                } while (i < charLen);
                i = charLen - charPos;
                if (sb == null) sb = new StringBuilder(i + 80);
                sb.Append(charBuffer, charPos, i);
            } while (ReadBuffer() > 0);
            string ss = sb.ToString();
            _lineLength = ss.Length;
            _bytesRead = _bytesRead + _lineLength;
            return ss;
        }
        // No data, class doesn't need to be serializable.
        // Note this class is threadsafe.
        private class NullStreamReader : StreamReaderWithPosition
        {
            public override Stream BaseStream => Stream.Null;
            public override Encoding CurrentEncoding => Encoding.Unicode;

            public override int Peek()
            {
                return -1;
            }
            public override int Read()
            {
                return -1;
            }

            public override int Read(char[] buffer, int index, int count)
            {
                return 0;
            }
            public override String ReadLine()
            {
                return null;
            }
            public override String ReadToEnd()
            {
                return String.Empty;
            }
        }
    }
}