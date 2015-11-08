using TailBlazer.Domain.Infrastructure;

namespace TailBlazer.Infrastucture
{
    public class ObjectProvider : IObjectProvider
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
    }
}
