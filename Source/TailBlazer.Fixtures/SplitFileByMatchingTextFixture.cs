using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Kernel;
using Xunit;

namespace TailBlazer.Fixtures
{
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

    public class Joined
    {
        
    }

    public static class SplitStringIntoMatches
    {
        public static IEnumerable<Matched> Match(this string source, IEnumerable<string> textToMatch)
        {
            return textToMatch.Select(source.Match);
        }

        public static Matched Match(this string source, string textToMatch)
        {
            var split = source.Split(new[] { textToMatch }, StringSplitOptions.None);
            return new Matched(source, split);
        }

        public static Joined Join(this Matched source)
        {
           //TODO: Join matching lines
            return new Joined();


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
            var stringsToMatch = new string[] {"cat","lazy"};
            var input = "The lazy cat could not catch a mouse";

            var split = input.Match(stringsToMatch).ToArray();
        }

        [Fact]
        public void FindWithNoMatch()
        {
            var stringsToMatch = new string[] { "dog", "energetic" };
            var input = "The lazy cat could not catch a mouse";

            var split = input.Match(stringsToMatch).ToArray();
        }

        [Fact]
        public void MatchAtEnd()
        {
            var stringsToMatch = new [] { "mouse" };
            var input = "The lazy cat could not catch a mouse";

            var split = input.Match(stringsToMatch).ToArray();
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
