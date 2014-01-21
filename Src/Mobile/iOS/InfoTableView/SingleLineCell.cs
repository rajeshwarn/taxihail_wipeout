using System.Drawing;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

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
            BackgroundView = new CustomCellBackgroundView(Frame);
            TextLabel.TextColor = UIColor.Black;
			TextLabel.BackgroundColor = UIColor.Clear;
            TextLabel.Font = UIFont.FromName(FontName.HelveticaNeueLight, 38/2);

			_rightImage = new UIImageView (new RectangleF (Frame.Width - 30, _sectionItem.RowHeight/2 - 15/2, 14, 15 ) ); 
			_rightImage.BackgroundColor = UIColor.Clear;
			_rightImage.ContentMode = UIViewContentMode.ScaleAspectFit;
            LoadImageFromAssets ( _rightImage, "right_arrow.png" );
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
		
		public void ReUse (SingleLineItem item)
		{
			if ( item != null )
			{
                _sectionItem = item;
				Load ();
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
