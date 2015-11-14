using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;

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
            IObservable<ScrollRequest> scrollRequest)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (textToMatch == null) throw new ArgumentNullException(nameof(textToMatch));
            
            //create list of lines which contain the observable text
           var matchedLines = textToMatch
                    .Select(searchText =>
                    {
                        Func<string, bool> predicate = null;
                        if (!string.IsNullOrEmpty(searchText))
                            predicate = s => s.Contains(searchText, StringComparison.OrdinalIgnoreCase);

                        return file.WatchFile().ScanFile(predicate);
                    }).Switch()
                    .Replay(1).RefCount();

            MatchedLines = matchedLines.Select(x => x.MatchingLines.Length);

            TotalLines = matchedLines.Select(x => x.TotalLines);

            //todo: plug in file missing or error into the screen
            
            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();
            
            //this is the beast! Dynamically combine lines requested by the consumer 
            //with the lines which exist in the file. This enables proper virtualisation of the file 
            var scroller = matchedLines
                .CombineLatest(scrollRequest, (scanResult, request) => new {scanResult , request })
                .Subscribe(x =>
                {
                    var mode = x.request.Mode;
                    var pageSize = x.request.PageSize;

                    var endOfTail = x.scanResult.EndOfTail;
                    var isInitial = x.scanResult.Index==0;
                    var allLines = x.scanResult.MatchingLines;


                    var previousPage = lines.Items.Select(l => new LineIndex(l.Number, l.Index)).ToArray();
                    
                    //If tailing, take the end only. 
                    //Otherwise take the page size and start index from the request
                    var currentPage = (mode == ScrollingMode.Tail
                        ? allLines.Skip(allLines.Length-pageSize).Take(pageSize).ToArray()
                        : allLines.Skip(Math.Min(x.request.FirstIndex-1, allLines.Length- pageSize)).Take(pageSize)).ToArray();
                    
                    var added = currentPage.Except(previousPage).ToArray();
                    var removed = previousPage.Except(currentPage).Select(li=>li.Line).ToArray();

                    if (added.Length + removed.Length == 0) return;

                    //TODO: Readline can throw an error, so need to hand this scenario


                    //read new lines from the file [TODO, need actual index in relative result set]
                    // var addedLines = file.ReadLines(added,i=> !isInitial && i > endOfTail).ToArray();
                    var addedLines = file.ReadLines(added, (lineIndex, text) =>
                    {
                        var isEndOfTail = !isInitial && lineIndex.Line > endOfTail;
                        return new Line(lineIndex.Line, lineIndex.Index, text, isEndOfTail ? DateTime.Now : (DateTime?)null);
                    }).ToArray();

                    //get old lines from the current collection
                    var removedLines = lines.Items.Where(l=> removed.Contains(l.Number)).ToArray();

                    //finally relect changes in the list
                    lines.Edit(innerList =>
                    {
                        innerList.RemoveMany(removedLines);
                        innerList.AddRange(addedLines);
                    });
                });
            _cleanUp = new CompositeDisposable(Lines, scroller, lines);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
