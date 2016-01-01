using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Kernel;
using FluentAssertions;
using Xunit;

namespace TailBlazer.Fixtures
{
    public class MatchedString : IEquatable<MatchedString>
    {
        public string Part { get; }
        public bool IsMatch { get; }

        public MatchedString(string part, bool isMatch)
        {
            Part = part;
            IsMatch = isMatch;
        }

        #region Equality

        public bool Equals(MatchedString other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Part, other.Part) && IsMatch == other.IsMatch;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MatchedString) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Part != null ? Part.GetHashCode() : 0)*397) ^ IsMatch.GetHashCode();
            }
        }

        public static bool operator ==(MatchedString left, MatchedString right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MatchedString left, MatchedString right)
        {
            return !Equals(left, right);
        }

        #endregion

        public override string ToString()
        {
            return $"{Part}, ({IsMatch})";
        }
    }

    public class Matched
    {
        public string Input { get; }
        public string[] Split { get; }

        public Matched(string input, string[] split)
        {
            Input = input;
            Split = split;
        }
    }

    public static class SplitStringIntoMatches
    {
        public static IEnumerable<Matched> Match(this string source, IEnumerable<string> textToMatch)
        {
            if (source == null) return Enumerable.Empty<Matched>();

            if (textToMatch == null)
                return new Matched[] {new Matched( source, new string[0]), };
                
            return textToMatch.Select(source.Match);
        }

        public static Matched Match(this string source, string textToMatch)
        {
            var split = source.Split(new[] { textToMatch }, StringSplitOptions.None);
            return new Matched(source, split);
        }

        public static IEnumerable<MatchedString> MatchString(this string source, string textToMatch)
        {
            return new StringMatchEnumerator(source, textToMatch);
        }
        public static IEnumerable<MatchedString> MatchString(this string source, IEnumerable<string> itemsToMatch)
        {
            return new StringMatchEnumerator(source, itemsToMatch);
        }
    }

    public class SplitFileByMatchingTextFixture
    {
        public SplitFileByMatchingTextFixture()
        {
        }


        [Fact]
        public void FindMatchingText()
        {
      
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("cat").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);

            var multimatched = input.MatchString(new string[] { "cat", "lazy" }).ToArray();
            var multijoined = multimatched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);
        }

        [Fact]
        public void FindWithNoMatch()
        {
            var stringsToMatch = new string[] { "dog", "energetic" };
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("energetic").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);
        }

        [Fact]
        public void MatchAtEnd()
        {
            var stringsToMatch = new [] { "mouse" };
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("mouse").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);
        }

        [Fact]
        public void MatchAtStart()
        {
            var stringsToMatch = new[] { "The" };
            var input = "The lazy cat could not catch a mouse";

            var matched = input.MatchString("The").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);
        }

        [Fact]
        public void NoMatch()
        {
            var input = "The lazy cat could not catch a mouse";
            var matched = input.MatchString("XXX").ToArray();
            var joined = matched.Select(m => m.Part).ToDelimited("");
            joined.Should().Be(input);
        }

        public class StringSplitter
        {
            public StringSplitter(string input, IEnumerable<string> textToMatch)
            {

                var matches = textToMatch.Select(t =>
                {
                    var split = input.Split(new[] {t}, StringSplitOptions.None);

                    return new Matched(t, split);
                }).ToArray();
                Console.WriteLine(matches);
            }


            //private IEnumerable<Matched> Process(string input, string[] split)
            //{
            //   yield return  new Matched();
            //} 

        }




        //public class Matched
        //{
        //    public string Input { get;  }
        //    public string[] Split { get;  }

        //    public Matched(string input, string[] split)
        //    {
        //        Input = input;
        //        Split = split;
        //    }
        //}
    }
}
