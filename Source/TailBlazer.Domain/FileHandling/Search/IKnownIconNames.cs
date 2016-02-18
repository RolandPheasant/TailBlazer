using System.Collections.Generic;

namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IKnownIconNames
    {
        string RegEx { get; }
        string Search { get; }
        string Selected { get; }

        IEnumerable<DefaultIcons> DefaultIcons();
    }

    public class DefaultIcons
    {
        public string Text { get; }
        public string IconName { get; }

        public DefaultIcons(string text, string iconName)
        {
            Text = text;
            IconName = iconName;
        }

    }
}