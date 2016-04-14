using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using DynamicData;
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
            var pulse = new Subject<Unit>();
            var scheduler = new TestScheduler();
            using (var file = new TestFile())
            {
                FileSearchResult fileSearchResult = null;

                using (file.Info
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchResult = x))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    pulse.Once();
                    fileSearchResult.Segments.Should().Be(1);
                    fileSearchResult.SegmentsCompleted.Should().Be(0);
                    fileSearchResult.IsSearching.Should().Be(false);
                    fileSearchResult.Count.Should().Be(0);
                }
            }
        }

        [Fact]
        public void EmptyTailedFiles()
        {
            var pulse = new Subject<Unit>();
            var scheduler = new TestScheduler();
            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                }
                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => str.Contains("9"), scheduler);

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    scheduler.AdvanceByMilliSeconds(250);
                    pulse.Once();
                    var segments = 0;
                    var segmentsCompleted = 0;
                    var isSearchCompleted = true;
                    var count = 0;
                    while (fileSearchResult != null)
                    {
                        segments += fileSearchResult.Segments;
                        segmentsCompleted += fileSearchResult.SegmentsCompleted;
                        isSearchCompleted = isSearchCompleted && fileSearchResult.IsSearching;
                        count += fileSearchResult.Count;
                        fileSearchResult = fileSearchResult.Previous as FileSearchResult;
                    }

                    segments.Should().Be(10);
                    segmentsCompleted.Should().Be(0);
                    isSearchCompleted.Should().Be(false);
                    count.Should().Be(0);
                }
            }
        }

        [Fact]
        public void NotExistingFile()
        {
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                file.Delete();

                FileSearchResult fileSearchResult = null;

                using (file.Info.WatchFile(pulse)
                    .Search(str => str.Contains("9"))
                    .Subscribe(x => fileSearchResult = x))
                {
                    fileSearchResult.Should().BeNull();
                }
            }
        }

        [Fact]
        public void NotExistingTailedFiles()
        {
            var pulse = new Subject<Unit>();
            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                }
                files.Dispose();

                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => str.Contains("9"));

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    fileSearchResult.Should().BeNull();
                }
            }
        }

        //[Fact]
        //Cannot recreate the file as something is hanging on to it.
        public void CreateFileLater()
        {
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                file.Delete();

                FileSearchResult fileSearchResult = null;

                using (file.Info.WatchFile(pulse)
                    .Search(str => str.Contains("9"))
                    .Subscribe(x => fileSearchResult = x))
                {
                    fileSearchResult.Should().Be(FileSearchResult.None);
                    file.Create();
                    pulse.Once();
                    fileSearchResult.Should().NotBe(FileSearchResult.None);
                }
            }
        }

        [Fact]
        public void SearchOnDataWhenFileIsPopulated()
        {
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                FileSearchResult fileSearchResult = null;


                using (file.Info.WatchFile(pulse)
                    .Search(str => str.Contains("9"))
                    .Subscribe(x => fileSearchResult = x))
                {
                    file.Append(Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());
                    pulse.Once();
                    pulse.Once();
                    fileSearchResult.Matches.Length.Should().NotBe(0);
                }
            }
        }

        [Fact]
        public void SearchOnDataWhenTailedFilesArePopulated()
        {
            var pulse = new Subject<Unit>();
            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                }
                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => str.Contains("9"));

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    for (int i = 0; i < 10; i++)
                    {
                        files[i].Append(Enumerable.Range(1, 100).Select(j => j.ToString()).ToArray());
                    }
                    pulse.Once();
                    pulse.Once();
                    while (fileSearchResult != null)
                    {
                        fileSearchResult.Matches.Length.Should().NotBe(0);
                        fileSearchResult = fileSearchResult.Previous as FileSearchResult;
                    }
                }
            }
        }

        [Fact]
        public void InitiallyEmptyThenLinesAdded()
        {
            var scheduler = new TestScheduler();

            using (var file = new TestFile())
            {
                FileSearchResult fileSearchResult = null;

                using (file.Info.WatchFile(scheduler: scheduler)
                    .Search(str => str.Contains("9"))
                    .Subscribe(x => fileSearchResult = x))
                {
                    scheduler.AdvanceBy(1);
                    file.Append(Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());
                    scheduler.AdvanceByMilliSeconds(250);
                    fileSearchResult.Matches.Length.Should().Be(19);
                }
            }
        }

        [Fact]
        public void WillContinuallyTail()
        {
            var scheduler = new TestScheduler();
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                FileSearchResult fileSearchResult = null;
                file.Append(Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

                using (file.Info.WatchFile(pulse)
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    fileSearchResult.Matches.Length.Should().Be(19);

                    file.Append(new[] { "9", "20" });
                    pulse.Once();
                    fileSearchResult.Matches.Length.Should().Be(20);

                    file.Append(new[] { "9999" });
                    pulse.Once();
                    fileSearchResult.Matches.Length.Should().Be(21);
                }
            }
        }

        [Fact]
        public void WillContinuallyTailInTailedFilesWithOneFileModified()
        {
            var pulse = new Subject<Unit>();

            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                    files[i].Append(Enumerable.Range(1, 100).Select(j => $"{(i + 1)}. files {j} row").ToArray());
                }
                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => str.Contains("files 9 row"));

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();

                    files[5].Append("6. files 9 row");
                    pulse.Once();
                    int matches = 0;
                    var _fileSearchResult = fileSearchResult;
                    while (_fileSearchResult?.Next != null)
                    {
                        _fileSearchResult = _fileSearchResult.Next as FileSearchResult;
                    }
                    while (_fileSearchResult != null)
                    {
                        matches += _fileSearchResult.Count;
                        _fileSearchResult = _fileSearchResult.Previous as FileSearchResult;
                    }

                    matches.Should().Be(11);
                }
            }
        }

        [Fact]
        public void WillContinuallyTailInTailedFilesWithTwoFileModified()
        {
            var pulse = new Subject<Unit>();

            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                    files[i].Append(Enumerable.Range(1, 100).Select(j => $"{(i + 1)}. files {j} row").ToArray());
                }
                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => str.Contains("files 9 row"));

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();

                    files[7].Append("8. files 9 row");
                    files[9].Append("10. files 9 row");

                    pulse.Once();

                    int matches = 0;
                    var _fileSearchResult = fileSearchResult;
                    while (_fileSearchResult?.Next != null)
                    {
                        _fileSearchResult = _fileSearchResult.Next as FileSearchResult;
                    }
                    while (_fileSearchResult != null)
                    {
                        matches += _fileSearchResult.Count;
                        _fileSearchResult = _fileSearchResult.Previous as FileSearchResult;
                    }
                    matches.Should().Be(12);
                }
            }
        }

        [Fact]
        public void CanReadLinesBack()
        {
            var scheduler = new TestScheduler();
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                FileSearchResult fileSearchResult = null;
                file.Append(Enumerable.Range(1, 100).Select(i => i.ToString()).ToArray());

                using (file.Info.WatchFile(pulse)
                    .Search(str => str.Contains("9"), scheduler)
                    .Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    fileSearchResult.Matches.Length.Should().Be(19);

                    file.Append(new[] { "9", "20" });
                    pulse.Once();
                    fileSearchResult.Matches.Length.Should().Be(20);

                    file.Append(new[] { "9999" });
                    pulse.Once();
                    fileSearchResult.Matches.Length.Should().Be(21);
                }
            }
        }

        [Fact]
        public void SearchInTailedFilesAndAfterAppendOneLineIntoOneFileAndSrollBegins()
        {
            var pulse = new Subject<Unit>();

            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                    files[i].Append(Enumerable.Range(1, 10).Select(j => $"{i}. files {j}. row").ToArray());
                }


                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => int.Parse(str.Split('.')[0]) % 2 != 0);

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    files[0].Append("1. file added new row");
                    pulse.Once();
                    while (fileSearchResult?.Next != null)
                    {
                        fileSearchResult = fileSearchResult?.Next as FileSearchResult;
                    }
                    var lines = fileSearchResult.ReadLines(new ScrollRequest(20, 0)).Select(line => line.Text);
                    var expectedLines = Enumerable.Range(1, 3).Select(i =>
                    {
                        return i % 2 != 0 ? Enumerable.Range(1, 10).Select(j => $"{i}. files {j}. row") : null;
                    })
                    .Where(t => t != null)
                    .SelectMany(t => t).ToList();
                    expectedLines.RemoveAt(expectedLines.Count - 1);
                    expectedLines =
                        new[] {new[] {"1. file added new row"}, expectedLines.ToArray()}.SelectMany(t => t).ToList();
                    lines.ShouldAllBeEquivalentTo(expectedLines.ToArray());
                }

            }
        }

        [Fact]
        public void SearchInTailedFilesAndScrollIntoTheResultBegins()
        {
            var pulse = new Subject<Unit>();

            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                    files[i].Append(Enumerable.Range(1, 10).Select(j => $"{i}. files {j}. row").ToArray());
                }
                

                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => int.Parse(str.Split('.')[0]) % 2 != 0);

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    var lines = fileSearchResult.ReadLines(new ScrollRequest(20, 0)).Select(line => line.Text);
                    var expectedLines = Enumerable.Range(1, 3).Select(i =>
                    {
                        return i%2 != 0 ? Enumerable.Range(1, 10).Select(j => $"{i}. files {j}. row") : null;
                    })
                    .Where(t => t != null)
                    .SelectMany(t => t).ToArray();

                    lines.ShouldAllBeEquivalentTo(expectedLines);
                }

            }
        }

        [Fact]
        public void SearchInTailedFilesAndScrollIntoTheAreaWhichContainsThreeFiles()
        {
            var pulse = new Subject<Unit>();

            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                    files[i].Append(Enumerable.Range(1, 10).Select(j => $"{i}. files {j}. row").ToArray());
                }


                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => int.Parse(str.Split('.')[0]) % 2 != 0);

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    var lines = fileSearchResult.ReadLines(new ScrollRequest(20, 5)).Select(line => line.Text);
                    var expectedLines = Enumerable.Range(1, 5).Select(i =>
                    {
                        if (i % 2 != 0)
                        {
                            return Enumerable.Range(1, 10).Select(j => i == 1 && j > 5
                                ? $"{i}. files {j}. row"
                                : (i == 5 && j < 6
                                    ? $"{i}. files {j}. row"
                                    : (i == 3 ? $"{i}. files {j}. row" : null))).Where(t => t != null);
                        }
                        return null;
                    })
                    .Where(t => t != null)
                    .SelectMany(t => t).ToArray();

                    lines.ShouldAllBeEquivalentTo(expectedLines);
                }

            }
        }

        [Fact]
        public void SearchInTailedFilesAndScrollIntoTheEnds()
        {
            var pulse = new Subject<Unit>();

            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                    files[i].Append(Enumerable.Range(1, 10).Select(j => $"{i}. files {j}. row").ToArray());
                }


                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => int.Parse(str.Split('.')[0]) % 2 != 0);

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    var lines = fileSearchResult.ReadLines(new ScrollRequest(20, 30)).Select(line => line.Text);
                    var expectedLines = Enumerable.Range(7, 3).Select(i =>
                    {
                        return i % 2 != 0 ? Enumerable.Range(1, 10).Select(j => $"{i}. files {j}. row") : null;
                    })
                    .Where(t => t != null)
                    .SelectMany(t => t).ToArray();

                    lines.ShouldAllBeEquivalentTo(expectedLines);
                }

            }
        }

        [Fact]
        public void SearchInTenTailedFiles()
        {
            var pulse = new Subject<Unit>();

            using (var files = new TestFileCollection())
            {
                for (int i = 0; i < 10; i++)
                {
                    files.Add(new TestFile());
                    files[i].Append(Enumerable.Range(1, 100).Select(j => $"{(i + 1)}. files {j} row").ToArray());
                }
                FileSearchResult fileSearchResult = null;
                var search = files.Select(t => t.Info.WatchFile(pulse))
                    .Merge()
                    .Search(str => str.Contains("files 9 row"));

                using (search.Subscribe(x => fileSearchResult = x))
                {
                    pulse.Once();
                    int matches = 0;
                    while (fileSearchResult != null)
                    {
                        matches += fileSearchResult.Count;
                        fileSearchResult = fileSearchResult.Previous as FileSearchResult;
                    }
                    matches.Should().Be(10);
                }
            }
        }

        //[Fact]
        public void CanReadLinesBackInLargeFile()
        {
            var scheduler = new TestScheduler();
            var pulse = new Subject<Unit>();

            using (var file = new TestFile())
            {
                IEnumerable<Line> lines = null;
                file.Append(Enumerable.Range(1, 100000).Select(i => i.ToString()).ToArray());

                using (file.Info.WatchFile(pulse)
                    .Search(str => str.Contains("9"), scheduler)
                    .Select(result => file.Info.ReadLinesByPosition(result.Matches).ToArray())
                    .Subscribe(x => lines = x))
                {
                    pulse.Once();
                    //fileSearchResult.Matches.Length.Should().Be(19);

                    file.Append(new[] { "9", "20" });
                    pulse.Once();
                    //  fileSearchResult.Matches.Length.Should().Be(20);

                    file.Append(new[] { "9999" });
                    pulse.Once();
                    //   fileSearchResult.Matches.Length.Should().Be(21);
                }
            }
        }
    }
}