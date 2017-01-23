using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Views.Tail;

namespace TailBlazer.Infrastucture.Virtualisation2
{
    public class VirtualisingController: IVirtualController<LineProxy>, IDisposable
    {
        private readonly ILineProxyFactory _proxyFactory;
        private readonly ISourceCache<LineProxy, int> _source = new SourceCache<LineProxy, int>(lp => lp.Index);
        private readonly IDisposable _cleanUp;
        private readonly int _pageSize = 50;
        private readonly object _locker = new object();
        private ILineProvider _lineProvider;
        private int _count =-1;

        public IObservable<int> CountChanged { get; }
        public IObservable<ItemWithIndex<LineProxy>[]> ItemsAdded { get; }

        public VirtualisingController(ILineProxyFactory proxyFactory, IObservable<ILineProvider> source, ISchedulerProvider schedulerProvider)
        {
            _proxyFactory = proxyFactory;       
            
            //1. when LP has changed, need to cache and update observable collection [tailing]
            //2. Scroll is governed by GetIndex();
            var initialTail = source.Synchronize(_locker).Publish();



            var loader = initialTail
                .Synchronize(_locker)
                .Do(lp=> _lineProvider = lp)
                .Scan(LastRead.Empty,(state,latest) =>
            {
                if (state == LastRead.Empty)
                {
                    var lines = LoadPage(new ScrollRequest(_pageSize)).ToArray();
                    var firstIndex = lines.Select(lp => lp.Index).Max();
                    var lastIndex = lines.Select(lr => lr.Start).Max();
                    return new LastRead(firstIndex, lastIndex, latest.Count, lines); 
                }
                else
                {
                    if (state.Count == latest.Count)
                        return new LastRead(state.FirstIndex, state.LastLineRead, latest.Count, new LineProxy[0]);

                    var toLoad = Math.Max(0,latest.Count - state.Count);
                    if (toLoad == 0)
                        return new LastRead(state.FirstIndex,state.LastLineRead,latest.Count, new LineProxy[0]);

                    var loaded = _lineProvider.ReadLines(new ScrollRequest(toLoad))
                                    .Select(line => _proxyFactory.Create(line))
                                    .ToArray();

                    if (loaded.Length ==0)
                        return state;

                    var firstIndex = loaded.Select(lp => lp.Index).Max();
                    var lastIndex = loaded.Select(lr => lr.Start).Max();
                    return new LastRead(firstIndex, lastIndex, latest.Count, loaded);
                }
            }).Subscribe(lines =>
                {
                    _source.AddOrUpdate(lines.Lines);
                });

            ItemsAdded = _source.Connect()
                .ObserveOn(schedulerProvider.MainThread)
                .Select(changes =>
                {
                    return changes
                        .Where(c => c.Reason == ChangeReason.Add)
                        .Select(lp => new ItemWithIndex<LineProxy>(lp.Current, lp.Current.Index))
                        .ToArray();
                })
                .ObserveOn(schedulerProvider.MainThread);

            CountChanged = initialTail.Select(lp => lp.Count).ObserveOn(schedulerProvider.MainThread); ;

            _cleanUp = new CompositeDisposable(_source, loader, initialTail.Connect());
        }

        private class LastRead : IEquatable<LastRead>
        {
            public int FirstIndex { get; }
            public long LastLineRead { get; }
            public int Count { get; }

            public LineProxy[] Lines { get; }

            public static readonly LastRead Empty = new LastRead(0,0,0,new LineProxy[0]);

            public LastRead(int firstIndex,long lastLineRead, int count, LineProxy[] lines)
            {
                FirstIndex = firstIndex;
                LastLineRead = lastLineRead;
                Count = count;
                Lines = lines;
            }

            #region equality

            public bool Equals(LastRead other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return FirstIndex == other.FirstIndex && Count == other.Count;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((LastRead) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (FirstIndex * 397) ^ Count;
                }
            }

            public static bool operator ==(LastRead left, LastRead right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(LastRead left, LastRead right)
            {
                return !Equals(left, right);
            }

            #endregion
        }



        private IEnumerable<LineProxy> LoadPage(ScrollRequest scrollRequest)
        {
            return _lineProvider.ReadLines(scrollRequest)
                .Select(_proxyFactory.Create);
        }

        public LineProxy Get(int index)
        {
            var item = _source.Lookup(index);
            if (item.HasValue)
                return item.Value;

            //try loading value
            var firstValue = Math.Max(0, index - (_pageSize/2));

            var lines = LoadPage(new ScrollRequest(ScrollReason.User,_pageSize, firstValue) ).ToArray();
            _source.AddOrUpdate(lines);

            return _source.Lookup(index).ValueOr(() => null);
        }
        
        public int IndexOf(LineProxy item)
        {
            return _source.Lookup(item.Index)
                .ConvertOr(lp => lp.Index,() => -1);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        public int Count()
        {
            return _lineProvider.Count;
        }
    }
}