using System;
namespace TaxiMobileApp
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

