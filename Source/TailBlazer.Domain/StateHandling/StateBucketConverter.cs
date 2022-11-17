using System.Xml.Linq;
using DynamicData.Kernel;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;

namespace TailBlazer.Domain.StateHandling;

public class StateBucketConverter : IConverter<StateBucket[]>
{
    private static class Structure
    {
        public const string Root = "Root";
        public const string Bucket = "Bucket";
        public const string Type = "Type";
        public const string Id = "Id";
        public const string Timestamp = "Timestamp";
            
        public const string InnerState = "ItemState";
        public const string Version = "Version";
        public const string Value = "Value";
    }

    public StateBucket[] Convert(State state)
    {
        if (state==null || state== State.Empty)
            return GetDefaultValue();

        var doc = XDocument.Parse(state.Value);
        var root = doc.ElementOrThrow(Structure.Root);

        var files = root.Elements(Structure.Bucket)
            .Select(element =>
            {
                var id = element.Attribute(Structure.Id).Value;
                var type = element.Attribute(Structure.Type).Value;
                var dateTime = element.Attribute(Structure.Timestamp).Value;

                var innerState = element.Element(Structure.InnerState);
                var stateValue = innerState.ElementOrThrow(Structure.Value);
                var stateVersion = innerState.Attribute(Structure.Version).Value
                    .ParseInt()
                    .ValueOr(()=>1);


                return new StateBucket(type,id,new State(stateVersion, stateValue), DateTime.Parse(dateTime).ToUniversalTime());
            }).ToArray();
        return files;
    }

    public State Convert(StateBucket[] buckets)
    {
        if (buckets == null || !buckets.Any())
            return State.Empty;

        var root = new XElement(new XElement(Structure.Root));
            
        var array = buckets.Select(bucket =>
        {
            var itemState = new XElement(new XElement(Structure.InnerState, new XAttribute(Structure.Version, bucket.State.Version)));
            itemState.Add(new XElement(Structure.Value, bucket.State.Value));
                
            return new XElement(Structure.Bucket,
                new XAttribute(Structure.Type, bucket.Type),
                new XAttribute(Structure.Id, bucket.Id),
                new XAttribute(Structure.Timestamp, bucket.TimeStamp),
                itemState);
        });

        foreach (var item in array)
        {
            root.Add(item);
        }
            
        var doc = new XDocument(root);
        return new State(2, doc.ToString());
    }

    public StateBucket[] GetDefaultValue()
    {
        return new StateBucket[0];
    }
}