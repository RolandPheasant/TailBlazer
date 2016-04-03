using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TailBlazer.Controls
{

    public class LinesListBox : ListBox
    {
        public static readonly DependencyProperty ObserveItemContainerGeneratorProperty = DependencyProperty.Register(
            "ObserveItemContainerGenerator", typeof (bool), typeof (LinesListBox), new PropertyMetadata(default(bool), OnObserveItemsContainerChangedPropertyChanged));

        public bool ObserveItemContainerGenerator
        {
            get { return (bool) GetValue(ObserveItemContainerGeneratorProperty); }
            set { SetValue(ObserveItemContainerGeneratorProperty, value); }
        }

        static LinesListBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LinesListBox), new FrameworkPropertyMetadata(typeof(LinesListBox)));
        }

        private readonly ISubject<bool> _subject = new Subject<bool>();     

        public LinesListBox()
        {

            var itemContainerStatuschanged = Observable.FromEventPattern<EventHandler, EventArgs>(h => this.ItemContainerGenerator.StatusChanged += h,h => this.ItemContainerGenerator.StatusChanged -= h)
                ;
          
             var loaded = Observable.FromEventPattern<RoutedEventHandler, RoutedEventArgs>
                (h => this.Loaded += h, h => this.Loaded -= h)
                .Subscribe(e =>
                {
                    //var observeItemContGenerator = _subject.StartWith(true)
                    //    .Where(x => x==true)
                    //    .Select(_ => Unit.Default);

                    //var itemContGeneratorStatusChanged = itemContainerStatuschanged
                    //    .Throttle(TimeSpan.FromMilliseconds(150), new DispatcherScheduler(this.Dispatcher))
                    //    .Select(_ => Unit.Default);


                    //Observable.Zip(observeItemContGenerator, itemContGeneratorStatusChanged)
                    //    .Subscribe(_ => this.FocusSelectedItem());


                    var itemContGeneratorStatusChanged = itemContainerStatuschanged
                        .Throttle(TimeSpan.FromMilliseconds(150), new DispatcherScheduler(this.Dispatcher))
                        .Subscribe(_ => FocusSelectedItem());

                });


        }

        private static void OnObserveItemsContainerChangedPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var indicator = (LinesListBox)sender;
            var newStatus = (bool)args.NewValue;
            indicator._subject.OnNext(newStatus);
        }

        private void FocusSelectedItem()
        {
            if (this.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated) return;
            //this.Log().Debug("ItemContainerGenerator status changed");
            if (this.Items.Count == 0) return;
           // var index = this.SelectedIndex;
            //if (index < 0) return;
            Action focusAction = () =>
            {
                ObserveItemContainerGenerator = false;
                if (this.Items.Count == 0) return;
                //index = this.SelectedIndex;
                //if (index < 0) return;

                //if (SelectedItem == null)
                //{
                //    SelectedItem = this.Items[0];
                //}

                this.Focus();
                //this.();

                return;
              //  ScrollIntoView(this.SelectedItem);
                var item = this.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                if (item == null) return;
                item.Focus();
                //SelectedItem = null;
                // this.Log().Debug("focus selected item {0} / {1}", index, item);
            };
            this.Dispatcher.BeginInvoke(DispatcherPriority.Background, focusAction);
        }

    }
}
