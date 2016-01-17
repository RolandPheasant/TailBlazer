using System.IO;
using FluentAssertions;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Settings;
using TailBlazer.Views.Options;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class SettingsConversion
    {
        [Fact]
        public void RecentFiles()
        {

            var files = new[]
            {
                new RecentFile(new FileInfo(@"C:\\File1.txt")),
                new RecentFile(new FileInfo(@"C:\\File2.txt")),
            };

            var converter = new RecentFilesToStateConverter();
            var state = converter.Convert(files);
            var restored = converter.Convert(state);
            restored.ShouldAllBeEquivalentTo(files);
        }

        [Fact]
        public void GeneralOptions()
        {

            var original = new GeneralOptions(Theme.Dark, false,0.5,125);
            var converter = new GeneralOptionsConverter();
            var state = converter.Convert(original);
            var restored = converter.Convert(state);
            restored.ShouldBeEquivalentTo(original);
        }
    }
}
