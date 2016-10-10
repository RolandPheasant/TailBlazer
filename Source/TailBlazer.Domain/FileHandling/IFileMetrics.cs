using System.Text;

namespace TailBlazer.Domain.FileHandling
{
    public interface IFileMetrics
    {
        string FullName { get; }
        string Name { get; }
        string Folder { get; }
        long Size { get; }

        Encoding Encoding { get; }
    }
}