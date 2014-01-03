using System.Drawing;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Common.Extensions;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class TitleView : UIView
	{
		private  UILabel _titleText;
		private UIImageView _img;

		public TitleView (UIView rightView, string title)
		{
			Initialize();
			Load( rightView, title, false);
		}

		public TitleView (UIView rightView, string title, bool hideLogo)
		{
			Initialize();
			Load( rightView, title, hideLogo);
		}

		private void Initialize()
		{
	        AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin;
	        AutosizesSubviews = false;

			Frame = new RectangleF(5, -3, 310, 50);

			_titleText = new UILabel (new RectangleF (0, 4, 320, 40)) 
			{
				TextAlignment =UITextAlignment.Center,
				Font = AppStyle.GetBoldFont ( 20 ),
				TextColor = AppStyle.NavigationTitleColor,
				BackgroundColor = UIColor.Clear,
			};

			AddSubview(_titleText);

			var image = UIImage.FromFile("Assets/Logo.png");
			_img = new UIImageView(image);

            if(  ( StyleManager.Current.CenterLogo.HasValue )  && ( StyleManager.Current.CenterLogo.Value ) )
            {
                _img.Frame = new RectangleF((320 -image.Size.Width)/2, 5, image.Size.Width, image.Size.Height );                     
            }
            else
            {
			    _img.Frame = new RectangleF(0, 5, image.Size.Width, image.Size.Height );            
            }
			_img.BackgroundColor = UIColor.Clear;
			_img.ContentMode = UIViewContentMode.ScaleAspectFit;
			_img.Hidden = true;
			AddSubview(_img);
		}

		private void Load( UIView rightView, string title, bool hideLogo )
		{
            if (rightView != null)
            {
                AddSubview(rightView);
            }
			else
			{
				SetTitle( title );
			}
			_img.Hidden = hideLogo;
		}


		public override RectangleF Frame {
            get { return base.Frame; }
            set
            { 
                if (_titleText != null & value.X > 0)
                {
                    _titleText.Frame = new RectangleF(-value.X, _titleText.Frame.Y, _titleText.Frame.Width, _titleText.Frame.Height);
                    if(  ( StyleManager.Current.CenterLogo.HasValue )  && ( StyleManager.Current.CenterLogo.Value ) )
                    {
                        var image = _img.Image;
                        _img.Frame = new RectangleF(((320 -image.Size.Width)/2)-value.X, 5, image.Size.Width, image.Size.Height );                     
                    }
                }
                base.Frame = value;
            }
		}

        public void SetTitle(string title)
        {
            if (title.HasValue())
            {
                _titleText.Text = title;
            }
        }

	}
}

