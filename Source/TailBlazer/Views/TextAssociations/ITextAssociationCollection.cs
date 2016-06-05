using DynamicData;

namespace TailBlazer.Views.TextAssociations
{
    public interface ITextAssociationCollection
    {
        IObservableList<TextAssociation> Items { get; }

        void MarkAsChanged(TextAssociation file);

    }
}