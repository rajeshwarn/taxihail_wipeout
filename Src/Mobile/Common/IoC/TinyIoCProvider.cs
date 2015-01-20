using System;
using System.Collections.Generic;
using Cirrious.CrossCore.Core;
using Cirrious.CrossCore.IoC;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.IoC
{
    public class TinyIoCProvider : MvxSingleton<IMvxIoCProvider>, IMvxIoCProvider
    {
        readonly TinyIoCContainer _container;
        readonly IDictionary<Type, List<Action>> _waiters = new Dictionary<Type, List<Action>>();
        readonly object _lockObject = new object();

        public TinyIoCProvider(TinyIoCContainer container)
        {
            _container = container;
        }

        public bool CanResolve<T>() where T : class
        {
            return _container.CanResolve<T>();
        }

        public bool CanResolve(Type type)
        {
            return _container.CanResolve(type);
        }

        public T Resolve<T>() where T : class
        {
            return _container.Resolve<T>();
        }

        public object Resolve(Type type)
        {
            return _container.Resolve(type);
        }

        public T Create<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public object Create(Type type)
        {
            throw new NotImplementedException();
        }

        public T GetSingleton<T>() where T : class
        {
            throw new NotImplementedException();
        }

        public object GetSingleton(Type type)
        {
            throw new NotImplementedException();
        }

        public bool TryResolve<T>(out T resolved) where T : class
        {
            return _container.TryResolve<T>(out resolved);
        }

        public bool TryResolve(Type type, out object resolved)
        {
            return _container.TryResolve(type, out resolved);
        }

        public void RegisterType<TFrom, TTo>() where TFrom : class where TTo : class, TFrom
        {
            _container.Register<TFrom, TTo>().AsMultiInstance();
            ExecuteCallback(typeof(TFrom));
        }

        public void RegisterType<TInterface>(Func<TInterface> constructor) where TInterface : class
        {
            throw new NotImplementedException();
        }

        public void RegisterType(Type t, Func<object> constructor)
        {
            throw new NotImplementedException();
        }

        public void RegisterType(Type tFrom, Type tTo)
        {
            _container.Register(tFrom, tTo).AsMultiInstance();
            ExecuteCallback(tFrom);
        }

        public void RegisterSingleton<TInterface>(TInterface theObject) where TInterface : class
        {
            _container.Register<TInterface>(theObject);
            ExecuteCallback(typeof(TInterface));
        }

        public void RegisterSingleton(Type tInterface, object theObject)
        {
            _container.Register(tInterface, theObject);
            ExecuteCallback(tInterface);
        }

        public void RegisterSingleton<TInterface>(Func<TInterface> theConstructor) where TInterface : class
        {
            var lazy = new Lazy<TInterface>(theConstructor);
            _container.Register<TInterface>((_, __) => lazy.Value);
            ExecuteCallback(typeof(TInterface));
        }

        public void RegisterSingleton(Type tInterface, Func<object> theConstructor)
        {
            var lazy = new Lazy<object>(theConstructor);
            _container.Register(tInterface, (_, __) => lazy.Value);
            ExecuteCallback(tInterface);
        }

        public T IoCConstruct<T>() where T : class
        {
            return _container.Resolve<T>(ResolveOptions.Default);
        }

        public object IoCConstruct(Type type)
        {
            return _container.Resolve(type);
        }

        public void CallbackWhenRegistered<T>(Action action)
        {
            CallbackWhenRegistered(typeof(T), action);
        }

        public void CallbackWhenRegistered(Type type, Action action)
        {
            lock (_lockObject)
            {
                if (!CanResolve(type))
                {
                    List<Action> actions;
                    if (_waiters.TryGetValue(type, out actions))
                    {
                        actions.Add(action);
                    }
                    else
                    {
                        actions = new List<Action> {action};
                        _waiters[type] = actions;
                    }
                    return;
                }
            }

            // if we get here then the type is already registered - so call the aciton immediately
            action();
        }

        private void ExecuteCallback(Type type)
        {
            List<Action> actions;
            lock (this)
            {
                if (_waiters.TryGetValue(type, out actions))
                {
                    _waiters.Remove(type);
                }
            }

            if (actions != null)
            {
                foreach (var action in actions)
                {
                    action();
                }
            }
        }
    }
}

