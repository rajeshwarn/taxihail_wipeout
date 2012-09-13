using System;
using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Common.Extensions;
using MonoTouch.CoreGraphics;
using apcurium.MK.Booking.Mobile.ListViewStructure;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public class SingleLineCell : UITableViewCell
	{
		private UIImageView _rightImage;
		private SingleLineItem _sectionItem;
		
		public SingleLineCell (IntPtr handle) : base(handle)
		{		
		}

//		[Export("initWithCoder:")]
//		public TwoLinesAddressCell (NSCoder coder) : base(coder)
//		{			
//		}
		
		public SingleLineCell (SingleLineItem data, string cellIdentifier) : base( UITableViewCellStyle.Default, new NSString(cellIdentifier)   )
		{					
			_sectionItem = data;
			
			SelectionStyle = UITableViewCellSelectionStyle.None;
			Accessory = UITableViewCellAccessory.None;
			Initialize ();
			Load ();
		}
		
		private void Initialize ()
		{

			BackgroundView = new CustomCellBackgroundView( _sectionItem.Index == 0, _sectionItem.Index == (_sectionItem.Parent.Items.Count() - 1), Frame, false );
			TextLabel.TextColor = AppStyle.CellFirstLineTextColor;
			TextLabel.BackgroundColor = UIColor.Clear;
			TextLabel.Font = AppStyle.CellFont;

			_rightImage = new UIImageView (new RectangleF (290, _sectionItem.RowHeight/2 - 15/2, 14, 15 ) ); 
			_rightImage.BackgroundColor = UIColor.Clear;
			_rightImage.ContentMode = UIViewContentMode.ScaleAspectFit;
			LoadImageFromAssets ( _rightImage, "Assets/Cells/rightArrow.png" );
			AddSubview ( _rightImage );	

		}

		
		public void Load ()
		{
			TextLabel.Text = _sectionItem.Label;
			UserInteractionEnabled = _sectionItem.Enabled();
		}

	

		public float GetHeight ()
		{
			return _sectionItem.RowHeight;
		}
		
		private void LoadImageFromAssets ( UIImageView imageView, string asset )
		{
			imageView.Image = UIImage. FromFile ( asset );	
		}
		
		public void ReUse ( SingleLineItem item )
		{
			if ( item != null )
			{
				if ( item != _sectionItem )
				{								
					_sectionItem = item;
				}			

				Load ();
				bool changed = false;

				if( ((CustomCellBackgroundView)BackgroundView).IsTop != (_sectionItem.Index == 0) )
				{
					((CustomCellBackgroundView)BackgroundView).IsTop = _sectionItem.Index == 0;
					changed = true;
				}
				if( ((CustomCellBackgroundView)BackgroundView).IsBottom != (_sectionItem.Index == (_sectionItem.Parent.Items.Count() - 1)) )
				{
					((CustomCellBackgroundView)BackgroundView).IsBottom = _sectionItem.Index == (_sectionItem.Parent.Items.Count() - 1);
					changed = true;
				}
				if( changed )
				{
					BackgroundView.SetNeedsDisplay();
				}

			}

		}
		
		public void CleanUp ()
		{					
			_sectionItem = null;		
			
			if ( _rightImage != null )
			{
				_rightImage.Image = new UIImage ();
				_rightImage.Dispose ();
				_rightImage = null;
			}						
		}
	
		public override void TouchesBegan ( NSSet touches, UIEvent evt )
		{	
			BackgroundView.SetNeedsDisplay();
			base.TouchesBegan ( touches, evt );	
		}
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			SetHighlighted( false, true );
			BackgroundView.SetNeedsDisplay();
			base.TouchesEnded (touches, evt);
		}
		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			SetHighlighted( false, true );
			BackgroundView.SetNeedsDisplay();
			base.TouchesCancelled (touches, evt);
		}


	}


}
