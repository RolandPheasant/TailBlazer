using Microsoft.Reactive.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using TailBlazer.Domain.FileHandling;
using Xunit;
using FluentAssertions;
using System.Reactive;
using TailBlazer.Domain.Infrastructure;
using System.IO;

namespace TailBlazer.Fixtures
{
    public class StartFromFixture
    {
        [Fact]
        public void EmptyForEmptyFile()
        {                        
            ILineProvider result = null;

            var pulse = new Subject<Unit>();            

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(0)
                .Subscribe(x => result = x))
            {
                pulse.Once();
                
                var lines = result.ReadLines(new ScrollRequest(10, 0L));
                lines.Should().HaveCount(0);                                
            }
        }

        [Fact]
        public void StartFromFirstPosition()
        {
            var firstLine = "This is the first line";

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(0)
                .Subscribe(x => result = x))
            {
                file.Append(firstLine);

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(10, 0L));
                lines.Should().HaveCount(1);
                lines.Single().Text.Should().Be(firstLine);
            }
        }        

        [Fact]
        public void NotATestJustTestingStuffOut()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the first line";

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(10)).ToArray();
                
            }
        }

        [Fact]
        public void SkipFirstLine()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var startPosition = (firstLine + Environment.NewLine).Length;

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(startPosition)
                .Subscribe(x => result = x))
            {
                
                file.Append(firstLine);
                file.Append(secondLine);                

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(10,0L)).ToArray();
                lines.Should().HaveCount(1);
                lines.Single().Text.Should().Be(secondLine);                
            }
        }

        [Fact]
        public void RelativeToOriginalScrollRequestPosition()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var startPosition = (firstLine + Environment.NewLine).Length;
            long originalScrollRequestPosition = 3;

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(startPosition)
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(10, originalScrollRequestPosition)).ToArray();
                lines.Should().HaveCount(1);
                lines.Single().Text.Should().Be(secondLine.Substring((int)originalScrollRequestPosition));
            }
        }

        [Fact]
        public void PageSize()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var thirdLine = "This is the third line";
            var fourthLine = "This is the fourth line";

            var startPosition = (firstLine + Environment.NewLine).Length;

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(startPosition)
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);
                file.Append(thirdLine);
                file.Append(fourthLine);

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(2, 0)).ToArray();
                lines.Should().HaveCount(2);
                lines.First().Text.Should().Be(secondLine);
                lines.Last().Text.Should().Be(thirdLine);
            }
        }

        [Fact]
        public void IndexFromLineOnStartPosition()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var thirdLine = "This is the third line";

            var startPosition = (firstLine + Environment.NewLine).Length;

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(startPosition)
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);
                file.Append(thirdLine);

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(2, 0L)).ToArray();

                for (int index = 0; index < lines.Length; index++)
                {
                    lines[index].Index.Should().Be(index);
                }

            }
        }

        [Fact]
        public void LineNumbering()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var thirdLine = "This is the third line";

            var startPosition = (firstLine + Environment.NewLine).Length;

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(startPosition)
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);
                file.Append(thirdLine);

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(2, 0L)).ToArray();

                for (int index = 0; index < lines.Length; index++)
                {
                    var expectedLineNumber = index + 1;
                    lines[index].Number.Should().Be(expectedLineNumber);
                }

            }
        }

        [Fact]
        public void LineStartAndEnd()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var thirdLine = "This is the third line";

            var startPosition = (firstLine + Environment.NewLine).Length;

            ILineProvider result = null;

            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(pulse)
                .Index()
                .StartFrom(startPosition)
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);
                file.Append(thirdLine);

                pulse.Once();

                var lines = result.ReadLines(new ScrollRequest(2, 0L)).ToArray();

                var expectedFirstLineStart = 0;
                var expectedFirstLineEnd = (secondLine + Environment.NewLine).Length;

                lines[0].LineInfo.Start.Should().Be(expectedFirstLineStart);
                lines[0].LineInfo.End.Should().Be(expectedFirstLineEnd);

                var expectedSecondLineStart = expectedFirstLineEnd;
                var expectedSecondLineEnd = expectedFirstLineEnd + (thirdLine + Environment.NewLine).Length;

                lines[1].LineInfo.Start.Should().Be(expectedSecondLineStart);
                lines[1].LineInfo.End.Should().Be(expectedSecondLineEnd);

            }
        }      
    }
    
}
