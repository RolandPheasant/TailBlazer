using System;
using System.Collections;
using System.Linq;
using System.Xml.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.FileHandling.TextAssociations;

public class TextAssociationToStateConverter: IConverter<TextAssociation[]>
{
    private static class Structure
    {
        public const string Root = "TextAssociationList";
        public const string TextAssociation = "TextAssociation";
        public const string Text = "Text";
        public const string UseRegEx = "UseRegEx";
        public const string Swatch = "Swatch";
        public const string Hue = "Hue";
        public const string Icon = "Icon";
        public const string IgnoreCase = "IgnoreCase";
        public const string Date = "Date";
    }

    public TextAssociation[] Convert(State state)
    {
        if (state == null || state == State.Empty)
            return new TextAssociation[0];

        var doc = XDocument.Parse(state.Value);
        var root = doc.ElementOrThrow(Structure.Root);


        var files = root.Elements(Structure.TextAssociation)
            .Select(element =>
            {
                var text = element.ElementOrThrow(Structure.Text);
                var useRegEx = element.Attribute(Structure.UseRegEx).Value.ParseBool().ValueOr(() => false);
                var ignoreCase = element.Attribute(Structure.IgnoreCase).Value.ParseBool().ValueOr(() => true);
                var swatch = element.Attribute(Structure.Swatch).Value;
                var hue = element.Attribute(Structure.Hue).Value;
                var icon = element.Attribute(Structure.Icon).Value;
                var dateTime = element.Attribute(Structure.Date).Value;

                return new TextAssociation( text, ignoreCase, useRegEx, swatch,icon,hue, DateTime.Parse(dateTime).ToUniversalTime());
            }).ToArray();
        return files;
    }

    public State Convert(TextAssociation[] items)
    {
        if (items == null || !items.Any())
            return State.Empty;
            
        var root = new XElement(new XElement(Structure.Root));

        var itemsNode = items.Select(f => new XElement(Structure.TextAssociation,
            new XElement(Structure.Text, f.Text),
            new XAttribute(Structure.UseRegEx, f.UseRegEx),
            new XAttribute(Structure.IgnoreCase, f.IgnoreCase),
            new XAttribute(Structure.Swatch, f.Swatch),
            new XAttribute(Structure.Hue, f.Hue),
            new XAttribute(Structure.Icon, f.Icon),
            new XAttribute(Structure.Date, f.DateTime)));
            
        itemsNode.ForEach(root.Add);

        XDocument doc = new XDocument(root);
        return new State(1, doc.ToString());
    }

    public TextAssociation[] GetDefaultValue()
    {
        return new TextAssociation[0];
    }
}