using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Reactive.Testing;
using TailBlazer.Domain.Infrastructure;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class ReorderingEventsTests : ReactiveTest
    {
        [Fact]
        public void ReorderingTest1()
        {
            var scheduler = new TestScheduler();

            var s1 = scheduler.CreateColdObservable(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(400, 3),
                OnNext(500, 4));

            var s2 = scheduler.CreateColdObservable(
                OnNext(100, 1),
                OnNext(200, 3),
                OnNext(300, 2),
                OnNext(500, 4));

            var results = scheduler.CreateObserver<int>();

            s1.OrderedCollectUsingMerge(
                right: s2,
                keySelector: i => i,
                firstKey: 1,
                nextKeyFunc: i => i + 1,
                resultSelector: (left, right) => left).Subscribe(results);

            scheduler.Start();

            results.Messages.AssertEqual(
                OnNext(100, 1),
                OnNext(300, 2),
                OnNext(400, 3),
                OnNext(500, 4));
        }

        [Fact]
        public void ReorderingTest2()
        {
            var scheduler = new TestScheduler();

            var s1 = scheduler.CreateColdObservable(
                OnNext(100, 1),
                OnNext(200, 2),
                OnNext(300, 3),
                OnNext(400, 4));

            var s2 = scheduler.CreateColdObservable(
                OnNext(100, 4),
                OnNext(200, 3),
                OnNext(300, 2),
                OnNext(400, 1));

            var results = scheduler.CreateObserver<int>();

            s1.OrderedCollectUsingZip(
                right: s2,
                keySelector: i => i,
                firstKey: 1,
                nextKeyFunc: i => i + 1,
                resultSelector: (left, right) => left).Subscribe(results);

            scheduler.Start();
            
            results.Messages.AssertEqual(
                OnNext(400, 1),
                OnNext(400, 2),
                OnNext(400, 3),
                OnNext(400, 4));
        }

        [Fact]
        public void SortTest()
        {
            var scheduler = new TestScheduler();

            var s1 = scheduler.CreateColdObservable(
                OnNext(55, 1),
                OnNext(49, 2),
                OnNext(13, 3),
                OnNext(77, 4));

            var results1 = scheduler.CreateObserver<int>();

            s1.Sort(
                keySelector: t => t,
                firstKey: 0,
                nextKeyFunc: i => i + 1
            ).Subscribe(results1);

            var s2 = scheduler.CreateColdObservable(
                OnNext(55, 1),
                OnNext(49, 2),
                OnNext(13, 3),
                OnNext(77, 4));

            var results2 = scheduler.CreateObserver<int>();

            s2.Sort(
                keySelector: t => t,
                firstKey: 0,
                nextKeyFunc: i => i + 1
            ).Subscribe(results2);

            scheduler.Start();

            results1.Messages.AssertEqual(results2.Messages);
        }
    }
}
