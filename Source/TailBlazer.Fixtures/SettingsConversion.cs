using System.Globalization;
using System.IO;
using System.Threading;
using FluentAssertions;
using TailBlazer.Domain.FileHandling.Recent;
using TailBlazer.Domain.Formatting;
using Xunit;
using TailBlazer.Views.Tail;
using TailBlazer.Domain.Settings;

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
                new RecentFile(new FileInfo(@"C:\\File2.txt"))
            };

            var converter = new RecentFilesToStateConverter();
            var state = converter.Convert(files);
            var restored = converter.Convert(state);
            restored.Should().BeEquivalentTo(files);
        }

        [Fact]
        public void GeneralOptionsWithCultureDeDe()
        {
            SerializeAndDeserializeWithCulture("de-DE");
        }

        [Fact]
        public void GeneralOptionsWithCultureEnUs()
        {
            SerializeAndDeserializeWithCulture("en-Us");
        }

        [Fact]
        public void EmptySearchShouldReturnDefault()
        {
            var converter = new SearchMetadataToStateConverter();
            var state = converter.Convert(State.Empty);
            state.Should().BeEquivalentTo(converter.GetDefaultValue());
        }

        [Fact]
        public void NullSearchShouldReturnDefault()
        {
            var converter = new SearchMetadataToStateConverter();
            State nullState = null;
            var state = converter.Convert(nullState);
            state.Should().BeEquivalentTo(converter.GetDefaultValue());
        }

        private void SerializeAndDeserializeWithCulture(string cultureName)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);

            var original = new GeneralOptions(Theme.Dark, false,0.5, 125,5, true, false);
            var converter = new GeneralOptionsConverter();
            var state = converter.Convert(original);
            var restored = converter.Convert(state);
            restored.Should().BeEquivalentTo(original);
        }
    }
}
