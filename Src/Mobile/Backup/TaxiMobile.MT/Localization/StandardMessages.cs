using TaxiMobile.Helper;

namespace TaxiMobile.Localization
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

