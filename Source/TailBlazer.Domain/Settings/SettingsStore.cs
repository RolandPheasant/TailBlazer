using System;
using System.IO;
using System.Xml.Linq;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Domain.Settings
{
    public class FileSettingsStore : ISettingsStore
    {
        private readonly ILogger _logger;

        private string Location { get; }

        private static class Structure
        {
            public const string Root = "Setting";
            public const string Version = "Version";
            public const string State = "State";
        }

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


            //var value = XDocument.Parse(state.Value);

            var root = new XElement(new XElement(Structure.Root, new XAttribute(Structure.Version,state.Version)));
            root.Add(new XElement(Structure.State, state.Value));


            var doc = new XDocument(root);
            var fileText = doc.ToString();

            _logger.Info($"Writing value {fileText}");
            File.WriteAllText(file, fileText);

        }

        public State Load(string key)
        {
            _logger.Info($"Reading setting for {key}");

            var file = Path.Combine(Location, $"{key}.setting");
            var info = new FileInfo(file);

            if (!info.Exists || info.Length == 0) return State.Empty;


            var doc = XDocument.Load(file);
            var root = doc.ElementOrThrow("Setting");
            var versionString = root.AttributeOrThrow("Version");
            var version = int.Parse(versionString);
            var state = root.ElementOrThrow("State");

            _logger.Info($"{key} has the value {state}");
            return new State(version, state);
        }
    }
}