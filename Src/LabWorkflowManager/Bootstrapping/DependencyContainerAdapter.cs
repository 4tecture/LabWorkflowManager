using _4tecture.UI.Common.DependencyInjection;
using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.UnityExtensions;

namespace LabWorkflowManager.Bootstrapping
{
    class DependencyContainerAdapeter : IDependencyContainer
    {
        private IUnityContainer container;
        public DependencyContainerAdapeter(IUnityContainer container)
        {
            this.container = container;
        }

        public IDependencyContainer RegisterInstance(Type t, string name, object instance)
        {
            this.container.RegisterInstance(t, name, instance);
            return this;
        }

        public IDependencyContainer RegisterType(Type from, Type to, string name, _4tecture.UI.Common.DependencyInjection.LifetimeManager lifetime, params object[] injectionMembers)
        {
            if (lifetime == _4tecture.UI.Common.DependencyInjection.LifetimeManager.Singleton)
            {
                if (from != null)
                {
                    if (injectionMembers != null && injectionMembers.Length > 0)
                    {
                        this.container.RegisterType(from, to, name, new ContainerControlledLifetimeManager(), new InjectionConstructor(injectionMembers));
                    }
                    else
                    {
                        this.container.RegisterType(from, to, name, new ContainerControlledLifetimeManager());
                    }
                }
                else
                {
                    if (injectionMembers != null && injectionMembers.Length > 0)
                    {
                        this.container.RegisterType(to, name, new ContainerControlledLifetimeManager(), new InjectionConstructor(injectionMembers));
                    }
                    else
                    {
                        this.container.RegisterType(to, name, new ContainerControlledLifetimeManager());
                    }
                }
            }
            else
            {
                if (from != null)
                {
                    if (injectionMembers != null && injectionMembers.Length > 0)
                    {
                        this.container.RegisterType(from, to, name, new InjectionConstructor(injectionMembers));
                    }
                    else
                    {
                        this.container.RegisterType(from, to, name);
                    }
                }
                else
                {
                    if (injectionMembers != null && injectionMembers.Length > 0)
                    {
                        this.container.RegisterType(to, name, new InjectionConstructor(injectionMembers));
                    }
                    else
                    {
                        this.container.RegisterType(to, name);
                    }
                }
            }

            return this;
        }

        public IDependencyContainer RegisterType<TFrom, TTo>(string name = null, _4tecture.UI.Common.DependencyInjection.LifetimeManager lifetime = _4tecture.UI.Common.DependencyInjection.LifetimeManager.PerRequest, params object[] injectionMembers)
        {
            return this.RegisterType(typeof(TFrom), typeof(TTo), name, lifetime, injectionMembers);
        }

        public IDependencyContainer RegisterType<TTo>(string name = null, _4tecture.UI.Common.DependencyInjection.LifetimeManager lifetime = _4tecture.UI.Common.DependencyInjection.LifetimeManager.PerRequest, params object[] injectionMembers)
        {
            return this.RegisterType(null, typeof(TTo), name, lifetime, injectionMembers);
        }

        public object Resolve(Type t, string name)
        {
            return this.container.Resolve(t, name);
        }

        public IEnumerable<object> ResolveAll(Type t)
        {
            return this.container.ResolveAll(t);
        }

        public bool IsTypeRegistered(Type type)
        {
            return this.container.IsRegistered(type);
        }

        public T TryResolve<T>()
        {
            return this.container.TryResolve<T>();
        }

        public object TryResolve(Type typeToResolve)
        {
            return this.container.TryResolve(typeToResolve);
        }



    }
}
