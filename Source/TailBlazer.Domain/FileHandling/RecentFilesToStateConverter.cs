using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.FileHandling
{
    public class RecentFilesToStateConverter: IConverter<RecentFile[]>
    {
        public RecentFile[] Convert(State state)
        {


            if (state == null || state == State.Empty)
                return new RecentFile[0];
            
            return null;
         //   return state.Value.FromDelimited(s => new RecentFile(s),Environment.NewLine).ToArray();
        }

        public State Convert(RecentFile[] files)
        {
            if (files == null || !files.Any())
                return State.Empty;
            
            var root = new XElement(new XElement("Files", new XAttribute("Version",1)));

            var fileNodeArray = files.Select(f => new XElement("File",
                new XAttribute("Name", f.Name),
                new XAttribute("Date", f.Timestamp)));

            fileNodeArray.ForEach(root.Add);

            XDocument doc = new XDocument(root);
            return new State(1, doc.ToString());
        }

        public RecentFile[] GetDefaultValue()
        {
            return new RecentFile[0];
        }
    }
}