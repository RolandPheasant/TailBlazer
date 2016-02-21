using System.Collections.Generic;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IKnownIcons
    {
        string GetIconFor(string text, bool useRegex);

    }

    public interface IColourSelector
    {

        string SelectFor(string text, bool useRegex);
    }


    public class DefaultIcons
    {
        public string Text { get; }
        public string IconName { get; }
        public bool MatchTextOnCase { get;  }


        public DefaultIcons(string text, string iconName, bool matchTextOnCase=true)
        {
            Text = text;
            IconName = iconName;
            MatchTextOnCase = matchTextOnCase;
        }

    }
}