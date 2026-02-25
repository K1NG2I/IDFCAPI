using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using StructureMap;

public class StructureMapDependencyResolver : IDependencyResolver
{
    private readonly IContainer _container;

    public StructureMapDependencyResolver(IContainer container)
    {
        _container = container;
    }

    public object GetService(Type serviceType)
    {
        if (serviceType.IsAbstract || serviceType.IsInterface)
        {
            return _container.TryGetInstance(serviceType);
        }
        return _container.GetInstance(serviceType);
    }

    public IEnumerable<object> GetServices(Type serviceType)
    {
        return _container.GetAllInstances(serviceType) as IEnumerable<object> ?? new List<object>();
    }

    public IDependencyScope BeginScope()
    {
        return this;
    }

    public void Dispose()
    {
        // Container disposal handled elsewhere if needed
    }
}