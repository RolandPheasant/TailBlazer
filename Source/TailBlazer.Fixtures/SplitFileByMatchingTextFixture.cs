using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Kernel;
using Xunit;

namespace TailBlazer.Fixtures
{
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

            var split = new StringSplitter(input, stringsToMatch);
        }

        public class StringSplitter
        {
            public StringSplitter(string input, IEnumerable<string> matches)
            {
                var xxx = input.Split(matches.AsArray(),StringSplitOptions.None);
                Console.WriteLine(xxx);
            }
        }

        public class Matched
        {
            
        }

    }
}
