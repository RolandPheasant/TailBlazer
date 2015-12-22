using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;
using DynamicData.Kernel;
using MaterialDesignColors;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Settings
{

    public class SeaarchOptionsViewModel
    {
        
    }

    public class SearchOptionsConverter : IConverter<SearchOptions>
    {
        private static class Structure
        {
            public const string Root = "Root";
            public const string SearchText = "SearchText";
            public const string Foreground = "Foreground";
            public const string Background = "Background";
            public const string Alert = "Alert";
        }

        public SearchOptions Convert(State state)
        {
            var defaults = this.GetDefaultValue();

            //var doc = XDocument.Parse(state.Value);
            //var root = doc.ElementOrThrow(Structure.Root);
            //var theme = root.ElementOrThrow(Structure.Foreground).ParseEnum<Theme>().ValueOr(() => defaults.Theme);
            //var highlight = root.ElementOrThrow(Structure.Background).ParseBool().ValueOr(() => defaults.HighlightTail);
            //var duration = root.ElementOrThrow(Structure.Duration).ParseDouble().ValueOr(() => defaults.HighlightDuration);
            //var scale = root.ElementOrThrow(Structure.Scale).ParseInt().ValueOr(() => defaults.Scale);
            //return new GeneralOptions(theme, highlight, duration, scale);

            return SearchOptions.None;
        }

        public State Convert(SearchOptions options)
        {
            var root = new XElement(new XElement(Structure.Root));
            root.Add(new XElement(Structure.SearchText, options.SearchText));
            root.Add(new XElement(Structure.Foreground, options.Foreground));
            root.Add(new XElement(Structure.Background, options.Background));
            root.Add(new XElement(Structure.Alert, options.Alert));
            var doc = new XDocument(root);
            var value = doc.ToString();
            return new State(1, value);
        }

        public SearchOptions GetDefaultValue()
        {
            return SearchOptions.None;
        }
    }

    public class SearchOptions
    {
        public static readonly SearchOptions None = new SearchOptions();

        public string SearchText { get;  }
        public Color Foreground { get;  }
        public Color Background { get;  }
        public bool Alert { get;  }

        public SearchOptions(string searchText, Color foreground, Color background, bool alert)
        {
            SearchText = searchText;
            Foreground = foreground;
            Background = background;
            Alert = alert;
        }

        private SearchOptions()
        {
        }
    }

    public class SwatchesProvider
    {
        public SwatchesProvider()
        {
            var assembly = Assembly.Load("MaterialDesignColors");
            var resourcesName = assembly.GetName().Name + ".g";
            var manager = new ResourceManager(resourcesName, assembly);
            var resourceSet = manager.GetResourceSet(CultureInfo.CurrentUICulture, true, true);
            var dictionaryEntries = resourceSet.OfType<DictionaryEntry>().ToList();
            var assemblyName = assembly.GetName().Name;

            var regex = new Regex(@"^themes\/materialdesigncolor\.(?<name>[a-z]+)\.(?<type>primary|accent)\.baml$");

            Swatches =
                dictionaryEntries
                .Select(x => new { key = x.Key.ToString(), match = regex.Match(x.Key.ToString()) })
                .Where(x => x.match.Success && x.match.Groups["name"].Value != "black")
                .GroupBy(x => x.match.Groups["name"].Value)
                .Select(x =>
                CreateSwatch
                (
                    x.Key,
                    Read(assemblyName, x.SingleOrDefault(y => y.match.Groups["type"].Value == "primary")?.key),
                    Read(assemblyName, x.SingleOrDefault(y => y.match.Groups["type"].Value == "accent")?.key)
                ))
                .ToList();

        }

        public IEnumerable<Swatch> Swatches { get; }

        private static Swatch CreateSwatch(string name, ResourceDictionary primaryDictionary, ResourceDictionary accentDictionary)
        {
            var primaryHues = new List<Hue>();
            var accentHues = new List<Hue>();

            if (primaryDictionary != null)
            {
                foreach (var entry in primaryDictionary.OfType<DictionaryEntry>()
                    .OrderBy(de => de.Key)
                    .Where(de => !de.Key.ToString().EndsWith("Foreground", StringComparison.Ordinal)))
                {
                    var colour = (Color)entry.Value;
                    var foregroundColour = (Color)
                        primaryDictionary.OfType<DictionaryEntry>()
                            .Single(de => de.Key.ToString().Equals(entry.Key.ToString() + "Foreground"))
                            .Value;

                    primaryHues.Add(new Hue(entry.Key.ToString(), colour, foregroundColour));
                }
            }

            if (accentDictionary != null)
            {
                foreach (var entry in accentDictionary.OfType<DictionaryEntry>()
                    .OrderBy(de => de.Key)
                    .Where(de => !de.Key.ToString().EndsWith("Foreground", StringComparison.Ordinal)))
                {
                    var colour = (Color)entry.Value;
                    var foregroundColour = (Color)
                        accentDictionary.OfType<DictionaryEntry>()
                            .Single(de => de.Key.ToString().Equals(entry.Key.ToString() + "Foreground"))
                            .Value;

                    accentHues.Add(new Hue(entry.Key.ToString(), colour, foregroundColour));
                }
            }

            return new Swatch(name, primaryHues, accentHues);
        }

        private static ResourceDictionary Read(string assemblyName, string path)
        {
            if (assemblyName == null || path == null)
                return null;

            return (ResourceDictionary)Application.LoadComponent(new Uri(
                $"/{assemblyName};component/{path.Replace(".baml", ".xaml")}",
                UriKind.RelativeOrAbsolute));
        }
    }

}
