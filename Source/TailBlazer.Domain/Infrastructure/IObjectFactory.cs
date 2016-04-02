using System;
using System.Collections.Generic;
using TailBlazer.Domain.Annotations;

namespace TailBlazer.Domain.Infrastructure
{
    public interface IObjectProvider
    {
        T Get<T>();
        T Get<T>(ExplictArg arg);
        T Get<T>(IEnumerable<ExplictArg> args)
            ;
    }

    public interface IObjectRegister
    {
        void Register<T>(T instance) where T : class;
    }


    public class ExplictArg
    {
        public string Key { get; }
        public object Arg { get; }

        public ExplictArg([NotNull] string key, [NotNull] object arg)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (arg == null) throw new ArgumentNullException(nameof(arg));
            Key = key;
            Arg = arg;
        }
    }
}
