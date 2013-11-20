using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4tecture.UI.Common.DependencyInjection
{
    public interface IDependencyContainer
    {
        IDependencyContainer RegisterInstance(Type t, object instance, string name = null);
        IDependencyContainer RegisterInstance<TFrom>(object instance, string name = null);
        IDependencyContainer RegisterType(Type from, Type to, string name, LifetimeManager lifetime, params object[] injectionMembers);
        IDependencyContainer RegisterType<TFrom, TTo>(string name = null, LifetimeManager lifetime = LifetimeManager.PerRequest, params object[] injectionMembers);
        IDependencyContainer RegisterType<TTo>(string name = null, LifetimeManager lifetime = LifetimeManager.PerRequest, params object[] injectionMembers);

        object Resolve(Type t, string name);

        IEnumerable<object> ResolveAll(Type t);

        bool IsTypeRegistered(Type type);
        T TryResolve<T>();
        object TryResolve(Type typeToResolve);
    }
}
