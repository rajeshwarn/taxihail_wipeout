using System;
using System.Collections.Generic;

namespace Microsoft.Practices.ServiceLocation
{

	public class ServiceLocator
	{

		private static ServiceLocator _current;

		public static ServiceLocator Current {
			get {
				
				if (_current == null) {
					_current = new ServiceLocator ();
				}
				return _current;
			}
		}

		private Dictionary<Type, Type> _registry = new Dictionary<Type, Type> ();
		private Dictionary<Type, object> _registrySingleInstance = new Dictionary<Type, object> ();

		private ServiceLocator ()
		{
		}

		
		public void RegisterSingleInstance2<TInterface> ( object instance )
		{
			_registrySingleInstance.Add (typeof(TInterface), instance);
			_registry.Add (typeof(TInterface), instance.GetType());
		}
		
		public void RegisterSingleInstance<TInterface, TConcret> ()
		{
			_registrySingleInstance.Add (typeof(TInterface), null);
			_registry.Add (typeof(TInterface), typeof(TConcret));
		}


		public void Register<TInterface, TConcret> ()
		{
			_registry.Add (typeof(TInterface), typeof(TConcret));
		}




		public T GetInstance<T> ()
		{			
			
			if (_registrySingleInstance.ContainsKey (typeof(T))) {
				if (_registrySingleInstance[typeof(T)] == null) {
					_registrySingleInstance[typeof(T)] = (T)Activator.CreateInstance (_registry[typeof(T)]);
				}
				return (T)_registrySingleInstance[typeof(T)];
			} else {
				return (T)Activator.CreateInstance (_registry[typeof(T)]);
			}
			
		}
		
		
		
		
		
		
	}
}

