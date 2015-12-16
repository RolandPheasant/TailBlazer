using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.Settings
{
    public class FileSettingsStore : ISettingsStore
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>
        /// The location.
        /// </value>
        private string Location { get; }
        public FileSettingsStore(ILogger logger)
        {
            _logger = logger;
            Location = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TailBlazer");

            var dir = new DirectoryInfo(Location);
            if (!dir.Exists) dir.Create();
            
            _logger.Info($"Settings folder is {Location}");
        }

        public void Save(string key, State state)
        {
            var file = Path.Combine(Location, $"{key}.setting");

            _logger.Info($"Creating setting for {key}");

            var writer = new StringWriter();
            using (var xmlWriter = new XmlTextWriter(writer))
            {
                using (xmlWriter.WriteElement("Setting"))
                {
                    xmlWriter.WriteAttributeString("Version",state.Version.ToString());
                    using (xmlWriter.WriteElement("State"))
                    {
                        xmlWriter.WriteString(state.Value);
                    }
                }
                xmlWriter.Close();
            }

            var formatted = XDocument.Parse(writer.ToString());

            _logger.Info($"Writing value {formatted.ToString()}");
            File.WriteAllText(file, formatted.ToString());

        }

        public State Load(string key)
        {
            _logger.Info($"Reading setting for {key}");

            var file = Path.Combine(Location, $"{key}.setting");

            var doc = XDocument.Load(file);
            var root = doc.Element("Setting");
            var versionString = root.AttributeOrThrow("Version");
            var version = int.Parse(versionString);
            var state = root.ElementOrThrow("State");

            _logger.Info($"{key} has the value {state}");
            return new State(version, state);
        }
    }
}