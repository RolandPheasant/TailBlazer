using System;
using DynamicData.Binding;

namespace TailBlazer.Infrastucture
{
    public class HeaderContent: AbstractNotifyPropertyChanged
    {
        public HeaderContent(string title)
        {
            Title = title;
        }

        public string Title { get; }
    }

    public class ViewContainer
    {
        public ViewContainer(string title, object content)
        {
            Header = new HeaderContent(title);
            Content = content;
        }

        public Guid Id { get; } = Guid.NewGuid();


        public object Header { get; }

        public object Content { get; }
    }
}