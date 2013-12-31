using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ListViewStructure;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public sealed class SingleLineCell : UITableViewCell
	{
		private UIImageView _rightImage;
		private SingleLineItem _sectionItem;
		
		
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

			_rightImage = new UIImageView (new RectangleF (Frame.Width - 30, _sectionItem.RowHeight/2 - 15/2, 14, 15 ) ); 
			_rightImage.BackgroundColor = UIColor.Clear;
			_rightImage.ContentMode = UIViewContentMode.ScaleAspectFit;
			LoadImageFromAssets ( _rightImage, "Assets/Cells/rightArrow.png" );
			_rightImage.Hidden = true;
			AddSubview ( _rightImage );	

		}

		
		public void Load ()
		{
			TextLabel.Text = _sectionItem.Label;
			_rightImage.Hidden = !_sectionItem.ShowRightArrow;
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
                _sectionItem = item;		

				Load ();
				var changed = false;

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
