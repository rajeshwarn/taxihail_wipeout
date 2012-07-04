using System;
using MonoTouch.UIKit;
using TaxiMobile.Localization;

namespace TaxiMobile.Helper
{
	public class MessageHelper
	{
		public MessageHelper ()
		{
		}
		
		public static void Show ( string title, string message, string additionalActionTitle , Action additionalAction )
		{
			AppContext.Current.Controller.InvokeOnMainThread ( delegate
			{					
				var av = new UIAlertView ( title, message, null, Resources.Close, additionalActionTitle );
				av.Clicked += delegate(object sender, UIButtonEventArgs e) {
				if (e.ButtonIndex == 1) {
				
						additionalAction();						
				}};
					
				av.Show (  );							
			} );
		}
		
		
		public static void Show ( string title, string message, Action onDismiss )
		{
			AppContext.Current.Controller.InvokeOnMainThread ( delegate
			{					
				var av = new UIAlertView ( title, message, null, Resources.Close, null );
				av.Dismissed += delegate {
					onDismiss();
				};
				av.Show (  );							
			} );
		}
		
		public static void Show ( string title, string message )
		{
			AppContext.Current.Controller.InvokeOnMainThread ( delegate
			{					
				var av = new UIAlertView ( title, message, null, Resources.Close, null );
				av.Show (  );
				
			} );
		}
		
		
		public static void Show ( string message )
		{
			AppContext.Current.Controller.InvokeOnMainThread ( delegate
			{									
				var av = new UIAlertView (  Resources.GenericTitle, message, null, Resources.Close, null );
				av.Show (  );
			} );
			
		}
	}
}

