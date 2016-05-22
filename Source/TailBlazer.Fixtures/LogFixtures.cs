using System.Collections.Generic;
using FluentAssertions;
using TailBlazer.Infrastucture;
using Xunit;

namespace TailBlazer.Fixtures
{
   public class LogFixtures
    {

        [Fact]
        public void LogNameDisplaysReadablyGenerics()
        {
            var subject = new List<int>();
            var logger = new Log4NetLogger(subject.GetType());
            logger.Name.Should().Be("List<Int32>");

        }

        [Fact]
        public void LogNameDisplayTakesTypeNameOnly()
        {
            var logger = new Log4NetLogger(typeof(int));
            logger.Name.Should().Be("Int32");

        }
    }
}
