using System;
using System.Threading;
using MonoTouch.Foundation;
using TaxiMobile.Diagnostics;

namespace TaxiMobile.Helper
{
	public class ThreadHelper
	{
		public ThreadHelper ()
		{
		}

		public static void ExecuteInThread (Action action)
		{
			//action();
			
			ThreadPool.QueueUserWorkItem (o =>
			{
				try
				{
					action ();
				}
				catch (Exception ex)
				{
					Logger.LogError( ex );
					throw;
				}
				
			});
		}

		public static Thread ExecuteInThreadWithPool (Action action)
		{
			
			
			var thread = new Thread (o =>
			{
				using (var ns = new NSAutoreleasePool ())
				{
					action ();
				}
			});
			
			thread.Start ();
			return thread;
		}
		
		
	}
}

