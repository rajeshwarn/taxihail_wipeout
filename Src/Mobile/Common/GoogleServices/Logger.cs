using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp.Lib.GoogleServices
{
	public class ServiceClientLogger
	{
		
		
		private static  Stack<Stopwatch> _watchs = new Stack<Stopwatch>();
		
		public static void Start( string message )
		{
			var w = new Stopwatch();
			_watchs.Push ( w );
			w.Start();			
			ServiceLocator.Current.GetInstance<ILogger> ().LogMessage (message);
		}
		
		
		public static void Stop( string message )
		{
			var w = _watchs.Pop ();
			w.Stop ();
			ServiceLocator.Current.GetInstance<ILogger> ().LogMessage (message  + " Execution time : " +  w.ElapsedMilliseconds.ToString () + " ms" );			
		}
		
		
		public static void LogStack()
		{			
			ServiceLocator.Current.GetInstance<ILogger> ().LogStack  ();		
		}
		
	}
}

