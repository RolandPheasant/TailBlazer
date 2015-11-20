using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling
{
    public class FileTailer: IDisposable
    {
        private readonly IDisposable _cleanUp;
        public IObservable<int> TotalLines { get;  }
        public IObservable<int> MatchedLines { get; }
        public IObservableList<Line> Lines { get; }

        public FileTailer(FileInfo file, 
            IObservable<string> textToMatch,
            IObservable<ScrollRequest> scrollRequest,
            IScheduler scheduler=null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (textToMatch == null) throw new ArgumentNullException(nameof(textToMatch));
            
            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();
            
            var matcher = textToMatch.Select(searchText =>
            {
                if (string.IsNullOrEmpty(searchText) || searchText.Length < 3)
                    return Observable.Return(LineMatches.None);

                return file.WatchFile(scheduler: scheduler)
                    .TakeWhile(notification => notification.Exists)
                    .Repeat()
                    .Match(s => s.Contains(searchText, StringComparison.OrdinalIgnoreCase));

            }).Switch()
            .Replay(1).RefCount();

            //temp mess for a few days
            var indexer = file.WatchFile(scheduler: scheduler)
                                .TakeWhile(notification => notification.Exists)
                                .Repeat()
                                .Index()
                                .Replay(1).RefCount();

            //count matching lines (all if no filter is specified)
            MatchedLines = indexer.CombineLatest(matcher, (indicies, matches) => matches == LineMatches.None ? indicies.Count : matches.Count);

            //count total line
            TotalLines = indexer.Select(x => x.Count);

            //todo: plug in file missing or error into the screen


            var locker = new object();
            var theBeast = indexer.Synchronize(locker)
                .CombineLatest(matcher.Synchronize(locker), scrollRequest.Synchronize(locker),(idx, mtch, scroll) => new CombinedResult(scroll, mtch, idx))
                .Select(result =>
                {
                    var scroll = result.Scroll;
                    var allLines = result.Incidies;
                    var matched = result.MatchedLines;

                    IEnumerable<LineIndex> indices;
                    if (result.MatchedLines.ChangedReason == LineMatchChangedReason.None)
                    {
                        indices = scroll.Mode == ScrollingMode.Tail
                            ? allLines.GetTail(scroll)
                            : allLines.GetFromIndex(scroll);
                    }
                    else
                    {
                        indices = scroll.Mode == ScrollingMode.Tail
                            ? allLines.GetTail(scroll, matched)
                            : allLines.GetFromIndex(scroll, matched);
                    }
                    
                    return  file.ReadLines(indices, (lineIndex, text) =>
                    {
                        var isEndOfTail = allLines.ChangedReason != LinesChangedReason.Loaded && lineIndex.Line > allLines.TailStartsAt;
                        return new Line(lineIndex.Line, lineIndex.Index, text,isEndOfTail ? DateTime.Now : (DateTime?) null);
                    }).ToArray();
                })
                //.RetryWithBackOff((error, attempts) =>
                //{
                //    //TODO: Log
                //    return TimeSpan.FromSeconds(1);
                //})
                .Subscribe(newPage =>
                {
                    //update observable list
                    lines.Edit(innerList =>
                    {

                        var removed = innerList.Except(newPage).ToArray();
                        var added = newPage.Except(innerList).ToArray();

                        if (removed.Any()) innerList.RemoveMany(removed);
                        if (added.Any())  innerList.AddRange(added);
                    });


                });

            ////this is the beast! Dynamically combine lines requested by the consumer 
            ////with the lines which exist in the file. This enables proper virtualisation of the file 
            //var scroller = matchedLines
            //    .CombineLatest(scrollRequest, (scanResult, request) => new {scanResult , request })
            //    .Subscribe(x =>
            //    {
            //        var mode = x.request.Mode;
            //        var pageSize = x.request.PageSize;
            //        var endOfTail = x.scanResult.EndOfTail;
            //        var isInitial = x.scanResult.Index==0;
            //        var allLines = x.scanResult.MatchingLines;
            //        var previousPage = lines.Items.Select(l => new LineIndex(l.Number, l.Index, 0, 0)).ToArray();
                    
            //        //Otherwise take the page size and start index from the request
            //        var currentPage = (mode == ScrollingMode.Tail
            //            ? allLines.Skip(allLines.Length-pageSize).Take(pageSize).ToArray()
            //            : allLines.Skip(x.request.FirstIndex).Take(pageSize)).ToArray();
                    
            //        var added = currentPage.Except(previousPage).ToArray();
            //        var removed = previousPage.Except(currentPage).Select(li=>li.Line).ToArray();

            //        if (added.Length + removed.Length == 0) return;


            //        try
            //        {
            //            var addedLines = file.ReadLines(added, (lineIndex, text) =>
            //            {
            //                var isEndOfTail = !isInitial && lineIndex.Line > endOfTail;
            //                return new Line(lineIndex.Line, lineIndex.Index, text, isEndOfTail ? DateTime.Now : (DateTime?)null);
            //            }).ToArray();

            //            //get old lines from the current collection
            //            var removedLines = lines.Items.Where(l => removed.Contains(l.Number)).ToArray();

            //            //finally relect changes in the list
            //            lines.Edit(innerList =>
            //            {
            //                innerList.RemoveMany(removedLines);
            //                innerList.AddRange(addedLines);
            //            });
            //        }
            //        catch (Exception)
            //        {
            //            //Very small chance of an error here but if one is causght the next successful read will recify this
            //            //TODO: 1. Feedback to user that steaming has stopped
            //            //TODO: 2. Place the ReadLines(..) method with the select of an observable
            //        }


            //    });
            _cleanUp = new CompositeDisposable(Lines, lines);
        }

        private class CombinedResult
        {
            public ScrollRequest Scroll { get;  }
            public LineMatches MatchedLines { get;  }
            public LineIndicies Incidies { get;  }

            public CombinedResult(ScrollRequest scroll, LineMatches matchedLines, LineIndicies incidies)
            {
                Scroll = scroll;
                MatchedLines = matchedLines;
                Incidies = incidies;
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
