using System;
using System.IO;
using System.Linq;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class StartFromFixture
    {
        [Fact]
        public void EmptyForEmptyFile()
        {
            FileNotification result = null;

            var scheduler = new TestScheduler();            

            using (var file = new TestFile())

            using (file.Info
                .WatchFile(scheduler: scheduler)
                .ScanFrom(0, scheduler: scheduler)
                .Subscribe(x => result = x))
            {
                scheduler.AdvanceByMilliSeconds(1000);

                var lines = File.ReadAllLines(result.FullName).ToArray();
                lines.Should().HaveCount(0);                                
            }
        }

        [Fact]
        public void StartAfterEndOfFileShouldReturnNothing()
        {
            FileNotification result = null;

            var scheduler = new TestScheduler();

            using (var file = new TestFile())

            using (file.Info
                .WatchFile(scheduler: scheduler)
                .ScanFrom(10000, scheduler: scheduler)
                .Subscribe(x => result = x))
            {
                file.Append("A");
                file.Append("B");
                file.Append("C");
                scheduler.AdvanceByMilliSeconds(1000);

                var lines = File.ReadAllLines(result.FullName).ToArray();
                lines.Should().HaveCount(0);
            }
        }


        [Fact]
        public void StartFromFirstPosition()
        {
            var firstLine = "This is the first line";

            FileNotification result = null;

            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(scheduler: scheduler)
                .ScanFrom(0, scheduler: scheduler)
                .Subscribe(x => result = x))
            {
                file.Append(firstLine);

                scheduler.AdvanceByMilliSeconds(1000);

                var lines = File.ReadAllLines(result.FullName).ToArray();
                lines.Should().HaveCount(1);
                lines.Single().Should().Be(firstLine);
            }
        }        


        [Fact]
        public void SkipFirstLine()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var startPosition = (firstLine + Environment.NewLine).Length;

            FileNotification result = null;

            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(scheduler: scheduler)
                .ScanFrom(startPosition, scheduler: scheduler)
                .Subscribe(x => result = x))
            {
                
                file.Append(firstLine);
                file.Append(secondLine);

                scheduler.AdvanceByMilliSeconds(1000);

                var lines = File.ReadAllLines(result.FullName).ToArray();
                lines.Should().HaveCount(1);
                lines.Single().Should().Be(secondLine);                
            }
        }


        [Fact]
        public void StartFromSecondLine()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var thirdLine = "This is the third line";
            var fourthLine = "This is the fourth line";

            var startPosition = (firstLine + Environment.NewLine).Length;
            var length = (firstLine + Environment.NewLine
                + secondLine + Environment.NewLine
                + thirdLine + Environment.NewLine
                + fourthLine + Environment.NewLine).Length - startPosition;

            FileNotification result = null;

            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(scheduler: scheduler)
                .ScanFrom(startPosition, scheduler: scheduler)
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);
                file.Append(thirdLine);
                file.Append(fourthLine);

                scheduler.AdvanceByMilliSeconds(1000);
                result.Size.Should().Be(length);
                var lines = File.ReadAllLines(result.FullName).ToArray();
                lines.Should().HaveCount(3);
 
            }
        }

        [Fact]
        public void StartFromBegining()
        {
            var firstLine = "This is the first line";
            var secondLine = "This is the second line";
            var thirdLine = "This is the third line";


            var length = (firstLine + Environment.NewLine
                + secondLine + Environment.NewLine
                + thirdLine + Environment.NewLine).Length;
            FileNotification result = null;

            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            using (file.Info
                .WatchFile(scheduler: scheduler)
                .ScanFrom(0, scheduler: scheduler)
                .Subscribe(x => result = x))
            {

                file.Append(firstLine);
                file.Append(secondLine);
                file.Append(thirdLine);

                scheduler.AdvanceByMilliSeconds(1000);

                var lines = File.ReadAllLines(result.FullName).ToArray();
                result.Size.Should().Be(length);
                lines.Should().HaveCount(3);
            }
        }

    }
    
}
