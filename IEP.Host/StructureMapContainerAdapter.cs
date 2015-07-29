    using ServiceStack.Configuration;
using StructureMap;

namespace IEP.Host
{
    public class StructureMapContainerAdapter : IContainerAdapter
    {
        public T TryResolve<T>()
        {
            return ObjectFactory.Container.TryGetInstance<T>();
        }

        public T Resolve<T>()
        {
            return ObjectFactory.Container.TryGetInstance<T>();
        }
    }
}