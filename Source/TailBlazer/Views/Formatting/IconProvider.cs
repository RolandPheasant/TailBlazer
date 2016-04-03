using System;
using System.Linq;
using System.Reactive.Disposables;
using DynamicData;
using MaterialDesignThemes.Wpf;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Views.Searching;

namespace TailBlazer.Views.Formatting
{
    public class IconProvider : IIconProvider, IDisposable
    {
        public IDefaultIconSelector DefaultIconSelector { get;  }

        private readonly IDisposable _cleanUp;
        private readonly ISourceList<IconDescription> _icons = new SourceList<IconDescription>();

        public IconProvider(IDefaultIconSelector defaultIconSelector)
        {
            DefaultIconSelector = defaultIconSelector;

            Icons = _icons.AsObservableList();

            var icons = Enum.GetNames(typeof(PackIconKind))
                        .Select(str =>
                        {
                            var value = (PackIconKind) Enum.Parse(typeof (PackIconKind), str);
                            return new IconDescription(value, str);
                        });

            _icons.AddRange(icons);

            _cleanUp = new CompositeDisposable(Icons, _icons);
        }

        public IObservableList<IconDescription> Icons { get; }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }
    }
}