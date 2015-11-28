using System;
using System.Reactive.Disposables;
using DynamicData.Binding;

namespace TailBlazer.Domain.Infrastructure
{
    internal sealed class HungryProperty<T> : AbstractNotifyPropertyChanged, IProperty<T>
    {
        private readonly IDisposable _cleanUp;
        private T _value;

        public HungryProperty(IObservable<T> source)
        {
            _cleanUp = source.Subscribe(t => Value = t);
        }

        public T Value
        {
            get { return _value; }
            set { SetAndRaise(ref _value, value); }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }

    internal sealed class LazyProperty<T> : AbstractNotifyPropertyChanged, IProperty<T>, IDisposable
    {

        private Lazy<IDisposable> factory;
        private readonly SingleAssignmentDisposable _cleanUp = new SingleAssignmentDisposable();
        private T _value;

        public LazyProperty(IObservable<T> source)
        {
            factory = new Lazy<IDisposable>(() => source.Subscribe(t => Value = t));

            //_cleanUp =
        }

        public T Value
        {
            get
            {
                EnsureLoaded();
                return _value;
            }
            private set
            {
               
                SetAndRaise(ref _value, value);
            }
        }

        private void EnsureLoaded()
        {
            _cleanUp.Disposable = factory.Value;
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        //}
    }
}
