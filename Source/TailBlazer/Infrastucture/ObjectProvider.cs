#region Usings

using System.Collections;
using System.Collections.Generic;
using StructureMap;
using StructureMap.Pipeline;
using TailBlazer.Domain.Infrastructure;
using Argument = TailBlazer.Domain.Infrastructure.Argument;

#endregion

namespace TailBlazer.Infrastucture;

public class ObjectProvider : IObjectProvider, IObjectRegister
{
    private readonly IContainer _container;

    public ObjectProvider(IContainer container)
    {
        _container = container;
    }

    public T Get<T>()
    {
        return _container.GetInstance<T>();
    }

    public T Get<T>(NamedArgument namedArgument)
    {
        return Get<T>(namedArgument.YieldOne());
    }

    public T Get<T>(IEnumerable<NamedArgument> explictArgs)
    {
        var args = new ExplicitArguments();
        foreach (var explictArg in explictArgs)
        {
            args.SetArg(explictArg.Key, explictArg.Instance);
        }
        return _container.GetInstance<T>(args);
    }

    public T Get<T>(IArgument arg)
    {
        return Get<T>(arg.YieldOne());
    }

    public T Get<T>(IEnumerable<IArgument> args)
    {
        return _container
            .With(x =>
            {
                foreach (var parameter in args)
                {
                    x.With(parameter.TargetType, parameter.Value);
                }
            })
            .GetInstance<T>();
    }

    public void Register<T>(T instance)
        where T : class
    {
        _container.Configure(x => x.For<T>().Use(instance));
    }
}