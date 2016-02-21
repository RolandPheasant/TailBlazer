namespace TailBlazer.Domain.FileHandling.Search
{
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