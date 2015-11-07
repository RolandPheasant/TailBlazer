using System;
using System.Collections.Generic;
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

        public IObservable<int[]> MatchedLines { get; }

        public IObservableList<Line> Lines { get; }

        public FileTailer(FileInfo file, 
            IObservable<string> textToMatch,
            IObservable<ScrollRequest> scrollRequest)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));
            if (textToMatch == null) throw new ArgumentNullException(nameof(textToMatch));
            
            //create list of lines which contain the observable text
            MatchedLines = file.ScanLineNumbers(textToMatch).Replay(1).RefCount();

            //count of lines
            TotalLines = file.CountLines();


            var lines = new SourceList<Line>();
            Lines = lines.AsObservableList();

            var scroller = MatchedLines
                .CombineLatest(scrollRequest, (matched, request) => new { matched , request })
                .Subscribe(x =>
                {
                    var mode = x.request.Type;
                    var numberOfItems = x.request.NumberOfItems;
                    var matched = x.matched;

                    var existing = lines.Items.Select(l => l.Number).ToArray();
                    var target = mode == ScrollingType.Tail
                        ? matched.Skip(numberOfItems).ToArray()
                        : matched.Skip(x.request.FirstIndex).Take(numberOfItems);



                });


            _cleanUp = new CompositeDisposable(Lines, scroller);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}
