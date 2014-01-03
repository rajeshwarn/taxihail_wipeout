using System;
using System.Threading;
using apcurium.MK.Booking.Mobile.Client.Diagnostics;

namespace apcurium.MK.Booking.Mobile.Client.Helper
{
	public static class ThreadHelper
	{
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
	}
}

