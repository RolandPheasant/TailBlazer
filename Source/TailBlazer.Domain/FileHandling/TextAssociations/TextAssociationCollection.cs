using System;
using System.Linq;
using System.Reactive.Disposables;
using DynamicData;
using DynamicData.Kernel;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.FileHandling.TextAssociations;

public class TextAssociationCollection : ITextAssociationCollection, IDisposable
{
    private readonly ILogger _logger;
    private readonly IDisposable _cleanUp;
    private readonly ISourceCache<TextAssociation, CaseInsensitiveString> _textAssociations = new SourceCache<TextAssociation, CaseInsensitiveString>(fi => fi.Text);

    public IObservableList<TextAssociation> Items { get; }

    public TextAssociationCollection(ILogger logger, ISetting<TextAssociation[]> setting)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Items = _textAssociations.Connect()
            .RemoveKey()
            .AsObservableList();

        var loader = setting.Value.Subscribe(files =>
        {
            _textAssociations.Edit(innerCache =>
            {
                //all files are loaded when state changes, so only add new ones
                var newItems = files
                    .Where(f => !innerCache.Lookup(f.Text).HasValue)
                    .ToArray();

                innerCache.AddOrUpdate(newItems);
            });
        });

        var settingsWriter = _textAssociations.Connect()
            .ToCollection()
            .Subscribe(items =>
            {
                setting.Write(items.ToArray());
            });

        _cleanUp = new CompositeDisposable(settingsWriter, loader, _textAssociations, Items);
    }

    public Optional<TextAssociation> Lookup(string key)
    {
        return _textAssociations.Lookup(key);
    }

    public void MarkAsChanged(TextAssociation file)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));

        _textAssociations.AddOrUpdate(file);
    }

    public void Remove(TextAssociation file)
    {
        _textAssociations.Remove(file);
    }

    public void Dispose()
    {
        _cleanUp.Dispose();
    }
}