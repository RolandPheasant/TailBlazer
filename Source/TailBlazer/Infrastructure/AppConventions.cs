using System.Collections;
using System.Linq;
using StructureMap;
using StructureMap.Graph;
using StructureMap.Graph.Scanning;

namespace TailBlazer.Infrastructure;

public class ViewModelFactoryConventions : IRegistrationConvention
{
    public void ScanTypes(TypeSet types, Registry registry)
    {

        // Only work on concrete types
        types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).ForEach(type =>
        {

            if (type.Name.EndsWith("Job"))
                registry.For(type).Singleton();
        });

    }
}

public class AppConventions : IRegistrationConvention
{
    public void ScanTypes(TypeSet types, Registry registry)
    {

        // Only work on concrete types
        types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).ForEach(type =>
        {

            type.GetInterfaces()
                .Where(@interface => @interface.Name == $"I{type.Name}")
                .ForEach(@interface => registry.For(@interface).Use(type).Singleton());

            if (type.Name.EndsWith("Job"))
                registry.For(type).Singleton();
        });

    }
}