using System.Windows;

namespace TailBlazer.Infrastucture
{
    public interface IDependencyObjectReceiver
    {
        void Receive(DependencyObject value);
    }


    public static class DependencyObjectHook
    {
        public static readonly DependencyProperty ReceiverProperty =
            DependencyProperty.RegisterAttached("Receiver", typeof(IDependencyObjectReceiver), typeof(DependencyObjectHook), new PropertyMetadata(default(IDependencyObjectReceiver), PropertyChanged));

        public static void SetReceiver(UIElement element, IDependencyObjectReceiver value)
        {
            element.SetValue(ReceiverProperty, value);
        }

        public static IDependencyObjectReceiver GetReceiver(UIElement element)
        {
            return (IDependencyObjectReceiver)element.GetValue(ReceiverProperty);
        }

        public static void PropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var receiver = args.NewValue as IDependencyObjectReceiver;

            receiver?.Receive(sender);
        }


    }
}