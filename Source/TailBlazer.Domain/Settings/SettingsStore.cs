using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace TailBlazer.Domain.Persistence
{
    class FileSettingsStore : ISettingsStore
    {

        private string Location { get; }
        public FileSettingsStore()
        {
            Location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TailBlazer");
        }

        public void Save(string key, State state)
        {

            var file = Path.Combine(Location, $"{key}.setting");

            //XDocument  and / or xmlTextWriter 
            //write the version [should be an attribute]
            //write for the  version [should be an Element]

            throw new NotImplementedException();

        }

        public State Load(string key)
        {

            var file = Path.Combine(Location, $"{key}.setting");

            //query the version [should be an attribute]
            //query for the  version [should be an Element]

            XDocument xmlFile = XDocument.Load(file);


            throw new NotImplementedException();
        }
    }
}