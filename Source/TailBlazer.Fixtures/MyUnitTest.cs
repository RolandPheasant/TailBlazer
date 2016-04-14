using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using Xunit;
using System.Reactive;
using System.Reactive.Subjects;
using System.IO;
using System.Diagnostics;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Fixtures
{
    public class MyUnitTest
    {
        [Fact]
        public void ScrollRequest_Test()
        {
            ScrollRequest sreq = new ScrollRequest(ScrollReason.Tail, 10, 100);
            sreq.Mode.ShouldBeEquivalentTo(ScrollReason.Tail);
        }

        [Fact]
        public void GetHashCode_Test()
        {
            ScrollRequest sreq = new ScrollRequest(ScrollReason.Tail, 10, 100);
            ScrollRequest sreq2 = new ScrollRequest(ScrollReason.User, 10, 100);
            sreq.GetHashCode().Should().NotBe(sreq2.GetHashCode());
        }

        [Fact]
        public void GetEncoding_Test()
        {
            var file = new TestFile();
            Encoding original = null;
            Encoding encode = file.Info.GetEncoding();

            using (var stream = File.Open(file.Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                using (var reader = new StreamReaderExtended(stream, true))
                {
                    var something = reader.Peek();
                    original = reader.CurrentEncoding;
                }
            }

            encode.Should().Be(original);
        }

        [Fact]
        public void GetFileLength_Test()
        {
            var file = new TestFile();
            long length = file.Info.GetFileLength();
            long original = 0;

            using (var stream = File.Open(file.Info.FullName, FileMode.Open, FileAccess.Read, FileShare.Delete | FileShare.ReadWrite))
            {
                original = stream.Length;
            }
        }
        
        [Fact]
        public void FileSegment_Test()
        {
            var fs = new FileSegment(1, 2, 10, FileSegmentType.Tail, null);
            var fs2 = new FileSegment(fs, 15);

            fs2.Index.ShouldBeEquivalentTo(fs.Index);
            fs2.Start.ShouldBeEquivalentTo(fs.Start);            
            fs2.End.ShouldBeEquivalentTo(15);
            fs2.Type.ShouldBeEquivalentTo(fs.Type);
            fs2.Key.Should().Be(fs.Key);
            fs2.Size.Should().Be(13);
        }

        [Fact]
        public void TailInfo_Test()
        {
            var tailinfo = new TailInfo(1000000);
            tailinfo.LastTail.ToShortDateString().Should().Be(DateTime.Now.ToShortDateString());
        }

        [Fact]
        public void ParseInt_Test()
        {
            String text = "1516";
            int number = (int)text.ParseInt();
            String newtext = number.ToString();
            text.Should().Be(newtext);
        }
              
        [Fact]
        public void ParseBool_Test()
        {
            String text = "True";
            bool truth = (bool)text.ParseBool();
            String newtext = truth.ToString();
            text.Should().Be(newtext);
        }

        [Fact]
        public void PrseDeciaml_Test()
        {
            String text = "10";
            decimal deci = (decimal)text.ParseDecimal();
            String newtext = deci.ToString();
            text.Should().Be(newtext);
        }

        [Fact]
        public void ParseDouble_Test()
        {
            String text = "10865902";
            double num = (double)text.ParseDouble();
            String newtext = num.ToString();
            text.Should().Be(newtext);
        }
    }
}
