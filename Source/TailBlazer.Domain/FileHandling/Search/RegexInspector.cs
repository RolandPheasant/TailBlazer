using System.Text.RegularExpressions;

namespace TailBlazer.Domain.FileHandling.Search
{
    public class RegexInspector
    {
        private readonly char[] _specialChars = @"\!@#$%^&*()[]+?".ToCharArray();

        private readonly Regex _isPlainText;

        public RegexInspector()
        {
            _isPlainText = new Regex("^[-][a-zA-Z0-9 ]*$");
        }

        public bool DoesThisLookLikeRegEx(string text)
        {
            return !string.IsNullOrEmpty(text) && !_isPlainText.IsMatch(text);
        }
    }
}