using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Settings
{
    public class RecentSearchToStateConverter: IConverter<RecentSearch[]>
    {
        private static class Structure
        {
            public const string Root = "Files";
            public const string File = "File";
            public const string Name = "Name";
            public const string Date = "Date";
        }

        public RecentSearch[] Convert(State state)
        {
            if (state == null || state == State.Empty)
                return new RecentSearch[0];

            var doc = XDocument.Parse(state.Value);

            var root = doc.ElementOrThrow(Structure.Root);
         
            var files = root.Elements(Structure.File)
                            .Select(element =>
                            {
                                var name = element.Attribute(Structure.Name).Value;
                                var dateTime = element.Attribute(Structure.Date).Value;
                                return new RecentSearch(DateTime.Parse(dateTime),name);
                            }).ToArray();
            return files;
        }

        public State Convert(RecentSearch[] files)
        {
            if (files == null || !files.Any())
                return State.Empty;
            
            var root = new XElement(new XElement(Structure.Root));

            var fileNodeArray = files.Select(f => new XElement(Structure.File,
                new XAttribute(Structure.Name, f.Text),
                new XAttribute(Structure.Date, f.Timestamp)));

            fileNodeArray.ForEach(root.Add);

            XDocument doc = new XDocument(root);
            return new State(2, doc.ToString());
        }

        public RecentSearch[] GetDefaultValue()
        {
            return new RecentSearch[0];
        }
    }
}