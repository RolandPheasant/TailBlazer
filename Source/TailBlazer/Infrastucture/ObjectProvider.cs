using System.Collections.Generic;
using System.Linq;
using StructureMap;
using StructureMap.Pipeline;
using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastucture
{
    public class ObjectProvider : IObjectProvider, IObjectRegister
    {
        private readonly StructureMap.IContainer _container;

        public ObjectProvider(StructureMap.IContainer container)
        {
            _container = container;

        }

        public T Get<T>()
        {
            return _container.GetInstance<T>();
        }

        public T Get<T>(ExplictArg explictArg)
        {
            var args = new ExplicitArguments();
            args.SetArg(explictArg.Key,explictArg.Arg);
            return _container.GetInstance<T>(args);
        }

        public T Get<T>(IEnumerable<ExplictArg> explictArgs)
        {
            var args = new ExplicitArguments();
            foreach (var explictArg in explictArgs)
            {
                args.SetArg(explictArg.Key, explictArg.Arg);
            }
            return _container.GetInstance<T>(args);
        }

        public void Register<T>(T instance)
            where T:class
        {
            _container.Configure(x => x.For<T>().Use(instance));
        }
    }
}
