using System.Collections.Generic;
using DynamicData.Kernel;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IKnownIconNames
    {
        string RegEx { get; }
        string Search { get; }
        string Selected { get; }

        string SelectIconFor(string text, bool useRegex);

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