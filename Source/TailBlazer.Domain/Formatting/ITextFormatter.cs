namespace TailBlazer.Domain.Formatting;

public interface ITextFormatter
{
    IObservable<IEnumerable<DisplayText>> GetFormatter(string inputText);
}