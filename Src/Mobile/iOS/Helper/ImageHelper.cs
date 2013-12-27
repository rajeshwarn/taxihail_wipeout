
using System;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class ImageHelper
	{
		public ImageHelper ()
		{
		}

		public static UIImage GetImage ( string imagePath )
		{
			
			if ( !imagePath.HasValue (  ) )
			{
				Logger.LogMessage ( "Value is null!" );
				return null;
			}
			
			UIImage result = null;
			
			result = UIImage.FromFile ( imagePath );
			
			return result;
			
		}

		public static void LoadImage ( byte[] image, UIImageView imageView )
		{
			
			LoadImage ( image, imageView, null );
			
			
		}

		public static void LoadImage ( byte[] image, UIImageView imageView, string pathDefault )
		{
			
			ThreadHelper.ExecuteInThread ( (  ) =>
			{
				try
				{
					
					var nsImage = NSData.FromArray ( image );
					UIImage loadedImage = null;
										
					if ( nsImage != null )
					{
						loadedImage = UIImage.LoadFromData ( nsImage );
					}


					else if ( pathDefault.HasValue (  ) )
					{
						loadedImage = UIImage.FromFile ( pathDefault );
					}
					
					imageView.InvokeOnMainThread ( (  ) => imageView.Image = loadedImage );
				}
				catch ( Exception ex )
				{
					Logger.LogError ( ex );
				}
			} );
			
			
		}

		public static void LoadImage ( string path, UIImageView image )
		{
			LoadImage ( path, image, null );
		}

		public static void LoadImage ( string path, UIImageView image, string pathDefault )
		{
			
			ThreadHelper.ExecuteInThread ( (  ) =>
			{
				try
				{
					NSData data = null;
					UIImage loadedImage = null;
					if ( path.HasValue (  ) )
					{
						data = NSData.FromUrl ( new NSUrl ( path ) );						
						if ( data != null )
						{
							loadedImage = UIImage.LoadFromData ( data );
						}
					}
					
					if ( ( ( data == null ) || ( loadedImage == null ) ) && ( pathDefault.HasValue (  ) ) )
					{
						loadedImage = UIImage.FromFile ( pathDefault );
					}
					
					image.InvokeOnMainThread ( (  ) => image.Image = loadedImage );
				}
				catch
				{
					
				}
			} );
			
			
		}
		
	}
}

