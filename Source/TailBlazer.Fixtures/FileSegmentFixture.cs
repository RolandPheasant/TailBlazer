using System;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileSegmentFixture
    {
        [Fact]
        public void FileChanged()
        {
            using (var file = new TestFile())
            {
                var info = file.Info;

                file.Append(1,1000);

                var scheduler = new TestScheduler();

                var segmenter = new FileSegmenter(info.WatchFile(scheduler: scheduler), 1000);
                FileSegmentCollection result = null;

                using (segmenter.Segments.Subscribe(segment => result = segment))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    result.Should().NotBeNull();
                    result.Count.Should().BeGreaterOrEqualTo(2);
                    result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Head);
                    result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Tail);
                    result.FileSize.Should().Be(info.Length);
                    
                    file.Append(101,10);
                    scheduler.AdvanceByMilliSeconds(250);
                    info.Refresh();
                    result.FileSize.Should().Be(info.Length);
                }
            }
        }

        [Fact]
        public void NotifiesOfSegmentWhenFileIsCreated()
        {
            //need to make this test
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                file.Delete();
                var info = file.Info;

                var segmenter = new FileSegmenter(info.WatchFile(scheduler: scheduler), 1000);
                FileSegmentCollection result = null;

                using (segmenter.Segments.Subscribe(segment => result = segment))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    file.Create();
                    scheduler.AdvanceByMilliSeconds(250);

                    result.Should().NotBeNull();

                    file.Append(1, 10000);
                    scheduler.AdvanceByMilliSeconds(250);

                    result.Should().NotBeNull();
                    result.Count.Should().BeGreaterOrEqualTo(2);
                    result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Head);
                    result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Tail);
                    result.FileSize.Should().Be(info.Length);


                    file.Append(101, 10);
                     scheduler.AdvanceByMilliSeconds(250);
                    info.Refresh();
                    result.FileSize.Should().Be(info.Length);
                }

            }
        }
    }
}