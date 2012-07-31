using System;
namespace apcurium.MK.Booking.Mobile.Client
{
	public static class StandardMessages
	{
		
		
		public static void ShowNotConnectedWarning (  )
		{
			AppContext.Current.Controller.InvokeOnMainThread( () => 
			{
				MessageHelper.Show( Resources.NoConnectionTitle, Resources.NoConnectionMessage  );
						
			} );
		}
	}
}

