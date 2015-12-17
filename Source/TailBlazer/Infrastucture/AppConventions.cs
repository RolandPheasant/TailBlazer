using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StructureMap.Configuration.DSL;
using StructureMap.Graph;
using StructureMap.TypeRules;

namespace TailBlazer.Infrastucture
{
    public class AppConventions : IRegistrationConvention
    {
        public void Process(Type type, Registry registry)
        {
            // Only work on concrete types
            if (!type.IsConcrete() || type.IsGenericType) return;

            // Add against all the interfaces implemented
            // by this concrete class
            type.GetInterfaces()
                .Where(@interface => @interface.Name == $"I{type.Name}" )
                .ForEach(@interface => registry.For(@interface).Use(type).Singleton());

            if (type.Name.EndsWith("Job"))
                registry.For(type).Singleton();
        }
    }
}