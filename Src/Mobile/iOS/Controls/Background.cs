using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public static class Background
	{
		private const int BackgroundViewTag = 91919191;

	    private static readonly Dictionary < string ,  UIImage> _images = new Dictionary < string ,  UIImage> ();
		
		public static void LoadForRegularView ( UIView view,  float topOffset = 0 )
		{
			RemoveBackgroundView( view );
			Load ( view, "Assets/Background.png", false, topOffset );	
		}
		
		public static void LoadForLandscape ( UIView view )
		{
			RemoveBackgroundView( view );
			Load ( view, "Assets/Background.png", false );	
		}
		
		public static void Load ( UIView view, string image, bool stretch, float topOffset = 0, float padding = 0 )
		{			
			var b = GetBackgroundView ( view, image, stretch, topOffset, padding );
			view.InsertSubview ( b, 0 );	
		}
		
		public static void RemoveBackgroundView( UIView view)
		{
			var backgroundViews = view.Subviews.Where ( s=>s.Tag == BackgroundViewTag );			
			backgroundViews.ForEach ( b=>b.RemoveFromSuperview());
			
		}
		public static UIView GetBackgroundView ( UIView view, string image, bool stretch, float topOffset = 0, float padding = 0 )
		{		
			var background = new UIImageView ();
			background.Tag = BackgroundViewTag;
			if ( !_images.ContainsKey ( image ) )
			{
				_images.Add ( image, UIImage. FromFile ( image ) ); 
			}
						
			background.Image = _images [image];
			
			
			if ( stretch )
			{
				background.ContentMode = UIViewContentMode.ScaleAspectFill;
			}
			else
			{
				background.ContentMode = UIViewContentMode.TopLeft;
			}
			background.Frame = new RectangleF( 0 + padding/2,0 + padding/2, view.Frame.Width - (padding), view.Frame.Height - (padding) );
			background.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			background.ClipsToBounds = true;
			
			if ( topOffset > 0 )
			{
				var r = new UIView ();
				r.Tag = BackgroundViewTag;
				r.Frame = view.Frame;
				background.Frame = new RectangleF (view.Frame.X, view.Frame.Y + topOffset, view.Frame.Width, view.Frame.Height + topOffset);
				r.AddSubview ( background );						
				return r;
			}
		    return background;
		}
	}
}

