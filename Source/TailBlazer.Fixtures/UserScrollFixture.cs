using System;
using System.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class UserScrollFixture
    {
        [Fact]
        public void ScrollToSpecifiedLocation()
        {
            var scheduler = new TestScheduler();
            var scrollRequest = new BehaviorSubject<ScrollRequest>(new ScrollRequest(ScrollReason.User,10,0));

            using (var file = new TestFile())
            {
                file.Append(CreateLines(1, 100));

                var autoTailer = new UserScroll(file.Info.WatchFile(scheduler: scheduler).Index(), scrollRequest);

                UserScrollResponse result = null;
                int counter = 0;
                using (autoTailer.Scroll().Subscribe(x => { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(250);
                    counter.Should().Be(1);
                    result.Count.Should().Be(10);
                    var expected = CreateLines(1, 10);
                    result.Lines.Select(l=>l.Text).ShouldBeEquivalentTo(expected);

                    file.Append(CreateLines(101, 5));
                    scheduler.AdvanceBySeconds(1);
                    counter.Should().Be(1);

                    scrollRequest.OnNext(new ScrollRequest(ScrollReason.User,15,20));
                    counter.Should().Be(2);
                    result.Count.Should().Be(15);
                     expected = CreateLines(21, 15);
                    result.Lines.Select(l => l.Text).ShouldBeEquivalentTo(expected);
                }
            }
        }

        [Fact]
        public void TailsLatestValuesOnly_ForFilteredValues()
        {
            var scheduler = new TestScheduler();
            var scrollRequest = new BehaviorSubject<ScrollRequest>(new ScrollRequest(ScrollReason.User, 5, 0));

            using (var file = new TestFile("TailsLatestValuesOnly_ForFilteredValues"))
            {
                file.Append(CreateLines(1, 100));

                var autoTailer = new UserScroll(file.Info.WatchFile(scheduler: scheduler).Search(str => str.Contains("9")), scrollRequest);

                UserScrollResponse result = null;
                int counter = 0;
                using (autoTailer.Scroll().Subscribe(x => { result = x; counter++; }))
                {
                    //check that intial values are loaded
                    scheduler.AdvanceBySeconds(1);
                    counter.Should().Be(1);
                    result.Count.Should().Be(5);
                    var expected = CreateLines(new int[] {9,19,29,39,49});
                    result.Lines.Select(l=>l.Text).ShouldBeEquivalentTo(expected);

                    //check that new lines do not force a scroll
                    file.Append(CreateLines(101, 10));
                    scheduler.AdvanceBySeconds(1);
                    counter.Should().Be(1);

                    //scroll to the 5th index in the result set
                    scrollRequest.OnNext(new ScrollRequest(ScrollReason.User, 5, 5));
                    counter.Should().Be(2);
                    result.Count.Should().Be(5);
                    expected = CreateLines(new int[] {  59,69,79,89,90 });
                    result.Lines.Select(l => l.Text).ShouldBeEquivalentTo(expected);
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