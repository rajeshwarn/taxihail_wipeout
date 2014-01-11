using TinyIoC;
using Cirrious.CrossCore.IoC;
using System;
using System.Collections.Generic;
using Cirrious.CrossCore.Core;

namespace apcurium.MK.Booking.Mobile.Mvx_
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

		#region IMvxIoCProvider implementation
		public bool CanResolve<T>() where T : class
		{
			return _container.CanResolve<T>();
		}
		public bool CanResolve(System.Type type)
		{
			return _container.CanResolve(type);
		}
		public T Resolve<T>() where T : class
		{
			return _container.Resolve<T>();
		}
		public object Resolve(System.Type type)
		{
			return _container.Resolve(type);
		}
		public T Create<T>() where T : class
		{
			throw new NotSupportedException();
		}
		public object Create(System.Type type)
		{
			throw new NotSupportedException();
		}
		public T GetSingleton<T>() where T : class
		{
			throw new NotSupportedException();
		}
		public object GetSingleton(System.Type type)
		{
			throw new NotSupportedException();
		}
		public bool TryResolve<T>(out T resolved) where T : class
		{
			return _container.TryResolve<T>(out resolved);
		}
		public bool TryResolve(System.Type type, out object resolved)
		{
			return _container.TryResolve(type, out resolved);
		}
		public void RegisterType<TFrom, TTo>() where TFrom : class where TTo : class, TFrom
		{
			_container.Register<TFrom, TTo>().AsMultiInstance();
		}
		public void RegisterType(System.Type tFrom, System.Type tTo)
		{
			_container.Register(tFrom, tTo).AsMultiInstance();
		}
		public void RegisterSingleton<TInterface>(TInterface theObject) where TInterface : class
		{
			_container.Register<TInterface>(theObject);
		}
		public void RegisterSingleton(System.Type tInterface, object theObject)
		{
			_container.Register(tInterface, theObject);
		}
		public void RegisterSingleton<TInterface>(System.Func<TInterface> theConstructor) where TInterface : class
		{
			var lazy = new Lazy<TInterface>(theConstructor);
			_container.Register<TInterface>((_, __) => lazy.Value);
		}
		public void RegisterSingleton(System.Type tInterface, System.Func<object> theConstructor)
		{
			var lazy = new Lazy<object>(theConstructor);
			_container.Register(tInterface, (_, __) => lazy.Value);
		}
		public T IoCConstruct<T>() where T : class
		{
			return _container.Resolve<T>(ResolveOptions.Default);
		}
		public object IoCConstruct(System.Type type)
		{
			return _container.Resolve(type);
		}
		public void CallbackWhenRegistered<T>(System.Action action)
		{
			CallbackWhenRegistered(typeof(T), action);
		}
		public void CallbackWhenRegistered(System.Type type, System.Action action)
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


		#endregion

    }
}

