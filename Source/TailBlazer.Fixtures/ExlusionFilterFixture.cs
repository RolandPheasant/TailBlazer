using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using DynamicData.Kernel;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.Annotations;
using TailBlazer.Domain.FileHandling;
using Xunit;

namespace TailBlazer.Fixtures
{
    
    public class ExludeLinesFixture
    {
        [Fact]
        public void ExludedFilesAre()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(Enumerable.Range(1, 20).Select(i => i.ToString()).ToArray());
                
                ILineProvider lineProvider = null;

                using (file.Info.Index(scheduler).Exclude(str => str.Contains("9")).Subscribe(x => lineProvider = x))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    lineProvider.Count.Should().Be(20);

                    var lines = lineProvider.ReadLines(new ScrollRequest(20, 0)).AsArray();
                    lines.Count().Should().Be(18);

                    var expected = Enumerable.Range(1, 20).Select(i => i.ToString()).Where(str => !str.Contains("9"));
                    lines.Select(line=>line.Text).Should().BeEquivalentTo(expected);
                }
            }
        }

        [Fact]
        public void WillCheckPreviousPage()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Append(Enumerable.Range(1, 20).Select(i => i.ToString()).ToArray());

                ILineProvider lineProvider = null;

                using (file.Info.Index(scheduler).Exclude(str => str.Contains("9")).Subscribe(x => lineProvider = x))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    lineProvider.Count.Should().Be(20);

                    var lines = lineProvider.ReadLines(new ScrollRequest(10)).AsArray();
                    lines.Count().Should().Be(9);

                    var expected = Enumerable.Range(1, 20)
                        .Select(i => i.ToString())
                        .Where(str => !str.Contains("9"))
                        .Reverse()
                        .Take(10);

                    //slines.Select(line => line.Text).Should().BeEquivalentTo(expected);
                }
            }
        }
    }
}
