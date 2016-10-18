using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileSearchFixture
    {
        [Fact]
        public void EmptyFile()
        {
            var scheduler = new TestScheduler();
            using (var file = new TestFile())
            {
                FileSearchCollection fileSearchCollection = null;

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchCollection = x))
                {
                    scheduler.AdvanceBySeconds(1);
                    fileSearchCollection.Total.Should().Be(1);
                    fileSearchCollection.Completed.Should().Be(1);
                    fileSearchCollection.IsSearching.Should().Be(false);
                    fileSearchCollection.Count.Should().Be(0);
                }
            }
        }

        [Fact]
        public void NotExistingFile()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Delete();

                FileSearchCollection fileSearchCollection = null;

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchCollection = x))
                {
                    scheduler.AdvanceBySeconds(1);
                    fileSearchCollection.Should().Be(FileSearchCollection.Empty);
                }
            }
        }

       [Fact]
        //Cannot recreate the file as something is hanging on to it.
        public void CreateFileLater()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Delete();

                FileSearchCollection fileSearchCollection = null;

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchCollection = x))
                {
                    fileSearchCollection.Should().BeNull();
                    file.Create();
                    file.Append(Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchCollection.Should().NotBe(FileSearchCollection.Empty);
                }
            }
        }

        [Fact]
        public void SearchOnDataWhenFileIsPopulated()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                FileSearchCollection fileSearchCollection = null;

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchCollection = x))
                {
                    file.Append(CreateLines(1, 100));
                    scheduler.AdvanceBySeconds(1);

                    fileSearchCollection.Matches.Length.Should().NotBe(0);
                    
                    var lines = fileSearchCollection.ReadLines(new ScrollRequest(5, 0)).ToArray();
                    var linesText = lines.Select(l => l.Text).ToArray();
                    var linesExpected = CreateLines(new [] {9,19,29,39,49});
                    linesText.ShouldAllBeEquivalentTo(linesExpected);

                    lines = fileSearchCollection.ReadLines(new ScrollRequest(5)).ToArray();
                    linesText = lines.Select(l => l.Text).ToArray();
                    linesExpected = CreateLines(new[] { 95, 96, 97, 98, 99 });
                    linesText.ShouldAllBeEquivalentTo(linesExpected);
                }
            }
        }
        [Fact]
        public void InitiallyEmptyThenLinesAdded()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                FileSearchCollection fileSearchCollection = null;

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchCollection = x))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    file.Append(CreateLines(1,100));
                    scheduler.AdvanceByMilliSeconds(500);
                    fileSearchCollection.Matches.Length.Should().Be(19);


                    var lines = fileSearchCollection.ReadLines(new ScrollRequest(5)).ToArray();
                    var linesText = lines.Select(l => l.Text).ToArray();
                    var linesExpected = CreateLines(new[] { 95, 96, 97, 98, 99 });
                    linesText.ShouldAllBeEquivalentTo(linesExpected);
                }
            }
        }

        [Fact]
        public void WillContinuallyTail()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                FileSearchCollection fileSearchCollection = null;
                file.Append(CreateLines(1,100));

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchCollection = x))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchCollection.Matches.Length.Should().Be(19);

                    file.Append(CreateLines(new int[] { 9,20 }));
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchCollection.Matches.Length.Should().Be(20);

                    file.Append(CreateLines(new int[] { 9999 }));
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchCollection.Matches.Length.Should().Be(21);

                    var lines = fileSearchCollection.ReadLines(new ScrollRequest(5)).ToArray();
                    var linesText = lines.Select(l => l.Text).ToArray();
                    var linesExpected = CreateLines(new[] {  97, 98, 99, 9,9999 });
                    linesText.ShouldAllBeEquivalentTo(linesExpected);
                }
            }
        }

        [Fact]
        public void CanReadLinesBack()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                FileSearchCollection fileSearchCollection = null;
                file.Append(1, 100);

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchCollection = x))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchCollection.Matches.Length.Should().Be(19);

                    file.Append(new[] { "9", "20" });
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchCollection.Matches.Length.Should().Be(20);

                    file.Append(new[] { "9999" });
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchCollection.Matches.Length.Should().Be(21);
                }
            }
        }

        [Fact]
        public void CanReadLinesBackInLargeFile()
        {
            var scheduler = new TestScheduler();
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                IEnumerable<Line> lines = null;
                file.Append(Enumerable.Range(1, 100000).Select(i => i.ToString()).ToArray());

                using (file.Info.WatchFile(scheduler: scheduler)
                    .SegmentWithReport()
                    .Search(str => str.Contains("9"), scheduler)
                    .Select(result => file.Info.ReadLinesByPosition(result.Matches).ToArray())
                    .Subscribe(x => lines=x))
                {
                    pulse.Once();



                    file.Append(new[] { "9", "20" });
                    pulse.Once();
                  //  fileSearchResult.Matches.Length.Should().Be(20);

                    file.Append(new[] { "9999" });
                    pulse.Once();
                 //   fileSearchResult.Matches.Length.Should().Be(21);
                }
            }
        }

        
        private string[] CreateLines(int start, int take)
        {
            return Enumerable.Range(start, take).Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
        }

        private string[] CreateLines(int[] lines)
        {
            return lines.Select(i => $"This is line number {i.ToString("00000000")}").ToArray();
        }
    }
}