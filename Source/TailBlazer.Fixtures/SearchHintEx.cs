using FluentAssertions;
using TailBlazer.Domain.FileHandling.Recent;
using Xunit;
using TailBlazer.Views.Searching;

namespace TailBlazer.Fixtures
{
    public class SearchHintEx
    {
        [Fact]
        public void ShouldAskForTextWhenTextIsEmpty()
        {
            var searchRequest = new SearchRequest("", false);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Type to search using plain text");
        }

        [Fact]
        public void ShouldAskForTextWhenRegexIsEmpty()
        {
            var searchRequest = new SearchRequest("", true);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Type to search using regex");
        }

        [Fact]
        public void ShouldBeValidWhenSearchingPlainText()
        {
            var searchRequest = new SearchRequest("[inf", false);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Hit enter to search using plain text");
        }

        [Fact]
        public void ShouldBeValidWhenSearchingAValidRegex()
        {
            var searchRequest = new SearchRequest("[inf]", true);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Hit enter to search using regex");
        }

        [Fact]
        public void ShouldBeInvalidWhenSearchingTooShortRegEx()
        {
            var searchRequest = new SearchRequest(".", true);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Regex must be at least 2 characters");
        }

        [Fact]
        public void ShouldBeValidWhenSearchingPlainTextExclusion()
        {
            var searchRequest = new SearchRequest("-[inf", false);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeTrue();
            result.Message.Should().Be("Hit enter to search using plain text");
        }

        [Fact]
        public void ShouldBeInvalidWhenPlainTextExclusionTextIsTooShort()
        {
            var searchRequest = new SearchRequest("-f", false);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Text must be at least 3 characters");
        }

        [Fact]
        public void ShouldBeInvalidWhenSearchingIrregularRegEx()
        {
            var searchRequest = new SearchRequest("[inf", true);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Invalid regular expression");
        }

        [Fact]
        public void ShouldBeInvalidWhenPlainTextContainsIllegalCharacter()
        {
            var searchRequest = new SearchRequest(@"[i\nf", false);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Text contains illegal characters");
        }

        [Fact]
        public void ShouldBeInvalidWhenPlainTextContainsOnlyWhiteSpaces()
        {
            var searchRequest = new SearchRequest("-    \t", false);

            var result = searchRequest.BuildMessage();

            result.IsValid.Should().BeFalse();
            result.Message.Should().Be("Text contains illegal characters");
        }
    }
}
