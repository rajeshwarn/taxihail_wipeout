//using System;
//using MonoTouch.UIKit;
//using System.Drawing;
//using apcurium.MK.Common.Extensions;
//
//namespace apcurium.MK.Booking.Mobile.Client
//{
//	public class AddressButton : UIButton
//	{
//		private UILabel _titleLabel;
//		private UILabel _addressLabel;
//		private string _placeholder = "";
//		private string _address = "";
//
//		public AddressButton ( RectangleF rect ) : base( rect )
//		{
//			BackgroundColor = UIColor.Clear;
//
//			_titleLabel = new UILabel( new RectangleF( 5, 3, rect.Width - 40, 10 ) );
//			_titleLabel.BackgroundColor = UIColor.Clear;
//			_titleLabel.Font = AppStyle.CellSmallFont;
//			_titleLabel.TextColor = UIColor.White;
//			this.AddSubview( _titleLabel );
//
//			_addressLabel = new UILabel( new RectangleF( 5, 16, rect.Width - 40, 15 ) );
//			_addressLabel.BackgroundColor = UIColor.Clear;
//			_addressLabel.Font = AppStyle.CellFont;
//			_addressLabel.TextColor = UIColor.White;
//			this.AddSubview( _addressLabel );
//
//			var arrow = new UIImageView( new RectangleF( Frame.Width - 35, rect.Height/2 - 15, 30, 30 ) );
//			arrow.BackgroundColor = UIColor.Clear;
//			arrow.Image = UIImage.FromFile( "Assets/VerticalButtonBar/rightArrow.png" );
//			this.AddSubview( arrow );
//
//		}
//
//		public void SetTitle ( string title )
//		{
//			_titleLabel.Text = title;
//		}
//
//		public void SetAddress( string address )
//		{
//			_address = address;
//			Refresh();
//		}
//
//		public void SetPlaceholder( string placeholder )
//		{
//			_placeholder = placeholder;
//			Refresh();
//		}
//
//		private void Refresh()
//		{
//			if( _address.IsNullOrEmpty() )
//			{
//				_addressLabel.Text = _placeholder;
//			}
//			else
//			{
//				_addressLabel.Text = _address;
//			}
//		}
//
//		public void Clear()
//		{
//			_address = "";
//			Refresh();
//		}
//
//		public string Text {
//			get { return _address; }
//			set { SetAddress( value ); }
//		}
//
//
////		public override void TouchesBegan (MonoTouch.Foundation.NSSet touches, UIEvent evt)
////		{
////			base.TouchesBegan (touches, evt);
////			Highlighted = true;
////			SetNeedsDisplay();
////		}
//
////		public override void Draw (RectangleF rect)
////		{
////			base.Draw (rect);
////
////			UIColor.Black.ColorWithAlpha( 0.5f );
////			backgroundColor.SetFill();
////			fillRectPath.LineWidth = _strokeSize;
////			fillRectPath.Fill();
////			}
////		}
//
//
//	}
//}
//
