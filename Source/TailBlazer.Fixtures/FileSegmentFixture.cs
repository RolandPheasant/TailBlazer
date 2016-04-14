using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class FileSegmentFixture
    {
        [Fact]
        public void MoreFileSegmentation()
        {
            TestFileCollection testFileCollection = new TestFileCollection();

            for (int i = 0; i < 10; i++)
            {
                testFileCollection.Add(new TestFile());
                testFileCollection[i].Append(
                    Enumerable.Range(1, 1000).Select(j => $"{i}. file and this is line number {j.ToString("00000000")}").ToArray());
            }

            var refresher = new Subject<Unit>();
            var segmenter = new FileSegmenter(testFileCollection.Select(t => t.Info.WatchFile(refresher)).Merge(), 1000);
            FileSegmentCollection result = null;

            using (var indexer = segmenter.Segments.Subscribe(segment => result = segment))
            {
                result.Should().NotBeNull();
                var current = new FileSegmentCollection( result );
                int depthOfLink = 0;
                while (current != null)
                {
                    depthOfLink++;
                    current = current.Link;
                }
                depthOfLink.Should().Be(10);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Head);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Tail);

                testFileCollection.ForEach(t => t.Delete());
            }

            testFileCollection.ForEach(t => t.Delete());

        }

        [Fact]
        public void FileChanged()
        {
            //need to make this test
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);

            File.AppendAllLines(file,
                Enumerable.Range(1, 1000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());

            var refresher = new Subject<Unit>();

            var segmenter = new FileSegmenter(info.WatchFile(refresher), 1000);
            FileSegmentCollection result = null;

            using (var indexer = segmenter.Segments.Subscribe(segment => result = segment))
            {
                result.Should().NotBeNull();
                result.Count.Should().BeGreaterOrEqualTo(2);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Head);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Tail);
                result.FileLength.Should().Be(info.Length);


                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));
                refresher.Once();
                info.Refresh();
                result.FileLength.Should().Be(info.Length);

                File.Delete(file);
            }

            File.Delete(file);
        }

        [Fact]
        public void NotifiesOfSegmentWhenFileIsCreated()
        {
            //need to make this test
            var file = Path.GetTempFileName();
            var info = new FileInfo(file);
            var refresher = new Subject<Unit>();

            var segmenter = new FileSegmenter(info.WatchFile(refresher), 1000);
            FileSegmentCollection result = null;

            using (var indexer = segmenter.Segments.Subscribe(segment => result = segment))
            {
                result.Should().NotBeNull();

                File.AppendAllLines(file,
                    Enumerable.Range(1, 10000).Select(i => $"This is line number {i.ToString("00000000")}").ToArray());
                refresher.Once();

                result.Should().NotBeNull();
                result.Count.Should().BeGreaterOrEqualTo(2);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Head);
                result.Segments.Select(fs => fs.Type).Should().Contain(FileSegmentType.Tail);
                result.FileLength.Should().Be(info.Length);


                File.AppendAllLines(file, Enumerable.Range(101, 10).Select(i => $"{i}"));
                refresher.Once();
                info.Refresh();
                result.FileLength.Should().Be(info.Length);

                File.Delete(file);
            }

            File.Delete(file);
        }
    }
}