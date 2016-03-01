using System.Collections;
using System.Linq;
using StructureMap;

using StructureMap.Graph;
using StructureMap.Graph.Scanning;
using TailBlazer.Views;


namespace TailBlazer.Infrastucture
{
    public class ViewModelFactoryConventions : IRegistrationConvention
    {
        public void ScanTypes(TypeSet types, Registry registry)
        {

            // Only work on concrete types
            types.FindTypes(TypeClassification.Concretes | TypeClassification.Closed).ForEach(type =>
            {

                // Only work on concrete types
                //   if (!type.IsConcrete() || type.IsGenericType) return;

                // Add against all the interfaces implemented
                // by this concrete class
                //type.GetInterfaces()
                //    .Where(@interface => type()  @interface.IsAic)
                //    .ForEach(@interface => registry.For(@interface).Use(type).Singleton());

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
}