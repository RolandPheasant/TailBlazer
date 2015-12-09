using System;
using DynamicData.Binding;

namespace TailBlazer.Infrastucture
{
    public class HeaderContent: AbstractNotifyPropertyChanged
    {
        private  string _title;

        public HeaderContent(string title)
        {
            _title = title;
        }

        public string Title
        {
            get { return _title; }
            set { SetAndRaise(ref _title,value);}
        }
    }

    public class ViewContainer
    {
        public Guid Id { get; } = Guid.NewGuid();


        public ViewContainer(string title, object content)
        {
            Header = new HeaderContent(title);
            Content = content;
        }

        public object Header { get; }

        public object Content { get; }
    }
}