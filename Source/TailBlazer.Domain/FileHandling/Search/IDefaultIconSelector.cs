namespace TailBlazer.Domain.FileHandling.Search;

public interface IDefaultIconSelector
{
    string GetIconFor(string text, bool useRegex);
    string GetIconOrDefault(string text, bool useRegex, string iconKind);

}