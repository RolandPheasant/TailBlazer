using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DynamicData;

namespace TailBlazer.Infrastucture
{
    /*
    
        TODO: Write custom selection logic.

        THis is required because the virtaualising logic removes

  */


    public interface IAttachedSelector
    {
        void Receive(Selector selector);
    }

    public static class SelectorHelper
    {
        public static readonly DependencyProperty BindingProperty = DependencyProperty.RegisterAttached("Binding", typeof(IAttachedSelector), typeof(SelectorHelper),
                new PropertyMetadata(default(IAttachedSelector), PropertyChanged));

        public static void SetBinding(Selector element, IAttachedSelector value)
        {
            element.SetValue(BindingProperty, value);
        }

        public static IAttachedSelector GetBinding(Selector element)
        {
            return (IAttachedSelector)element.GetValue(BindingProperty);
        }

        public static void PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var receiver = args.NewValue as IAttachedSelector;
            receiver?.Receive((Selector)sender);
        }
    }

    public class SelectionController<T> : IDisposable, IAttachedSelector
    {
        private readonly ISourceList<T> _sourceList = new SourceList<T>();
        private readonly IDisposable _cleanUp;
        private readonly SerialDisposable _serialDisposable = new SerialDisposable();

        private bool _isSelecting;
        private Selector _selector;


        public SelectionController()
        {
            _cleanUp = new CompositeDisposable(_sourceList, _serialDisposable);


            _sourceList.Connect()
                .ToCollection()
                .Subscribe(colection =>
                {
                    var selected = colection.Count;
                   // var text = colection.Select(t=>t).ToDelimited();
                   // Console.WriteLine($"Selected={selected}. {text} ");
                    Console.WriteLine($"Selected={selected}");
                });

        }

        void IAttachedSelector.Receive(Selector selector)
        {
            _selector = selector;



            var changed = selector.ItemsSource as INotifyCollectionChanged;
            changed.CollectionChanged += Changed_CollectionChanged;

            //clear selection when the mouse is clicked and no other key is pressed
            var mouseDown = Observable.FromEventPattern<MouseButtonEventHandler, MouseButtonEventArgs>(
                h => selector.PreviewMouseLeftButtonDown += h,
                h => selector.PreviewMouseLeftButtonDown -= h)

                .Select(evt => evt.EventArgs)
                .Subscribe(mouseArgs =>
                {
                    Console.WriteLine(mouseArgs.ButtonState);

                    var isKeyDown = (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)
                     || Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightCtrl));


                    if (!isKeyDown)
                        _sourceList.Clear();
                });

            _serialDisposable.Disposable = Observable
                .FromEventPattern<SelectionChangedEventHandler, SelectionChangedEventArgs>(
                    h => selector.SelectionChanged += h,
                    h => selector.SelectionChanged -= h)
                .Select(evt => evt.EventArgs)
                .Subscribe(HandleSelectionChanged);
        }

        private void Changed_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var toSelect = _sourceList.Items.ToArray();

            if (e.NewItems == null)
                return;

            foreach (var item in e.NewItems)
            {
                if (toSelect.Contains((T)item))
                {
                    if (!((ListBox)_selector).SelectedItems.Contains(item))
                        ((ListBox)_selector).SelectedItems.Add(item);
                }
            }

        }

        public IObservableList<T> SelectedItems => _sourceList;

        private void HandleSelectionChanged(SelectionChangedEventArgs args)
        {
            //Logic - by default when items scroll out of view they are no longer selected.
            //this is because the panel is virtualised and and automatically unselected due
            //to the control thinking that the item is not longer part of the overall collection


            //Solution= when user clicks on a row, start a new selection session.
            //otherwise continue to add user selection

         //  Console.WriteLine(args);

            args.Handled = true;

            if (_isSelecting) return;
            try
            {
                _isSelecting = true;

                _sourceList.Edit(list =>
                {
                    list.AddRange(args.AddedItems.OfType<T>().ToList());

                    //only remove is key is not pressed down


                });
            }
            finally
            {
                _isSelecting = false;
            }
        }

        public void Select(T item)
        {
            if (_selector == null) return;

            if (!_selector.Dispatcher.CheckAccess())
            {
                _selector.Dispatcher.BeginInvoke(new Action(() => Select(item)));
                return;
            }


            if (_selector is ListView)
            {
                ((ListView)_selector).SelectedItems.Add(item);
            }
            else if (item is MultiSelector)
            {
                ((MultiSelector)_selector).SelectedItems.Add(item);
            }
            else
            {
                _selector.SelectedItem = item;
            }

        }

        public void DeSelect(T item)
        {
            if (_selector == null) return;
            if (!_selector.Dispatcher.CheckAccess())
            {
                _selector.Dispatcher.BeginInvoke(new Action(() => DeSelect(item)));
                return;
            }

            if (_selector is ListView)
            {
                ((ListView)_selector).SelectedItems.Remove(item);
            }
            else if (_selector is MultiSelector)
            {
                ((MultiSelector)_selector).SelectedItems.Remove(item);
            }
            else
            {
                _selector.SelectedItem = null;
            }
        }

        public void Clear()
        {
            if (_selector == null) return;
            if (!_selector.Dispatcher.CheckAccess())
            {
                _selector.Dispatcher.BeginInvoke(new Action(Clear));
                return;
            }

            if (_selector is ListView)
            {
                ((ListView)_selector).SelectedItems.Clear();
            }
            else if (_selector is MultiSelector)
            {
                ((MultiSelector)_selector).SelectedItems.Clear();
            }
            else
            {
                _selector.SelectedItem = null;
            }
        }





        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}