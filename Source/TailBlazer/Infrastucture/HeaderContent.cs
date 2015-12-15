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
}