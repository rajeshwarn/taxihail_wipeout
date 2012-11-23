using System;
using MonoTouch.UIKit;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client
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

			Frame = new System.Drawing.RectangleF(5, -3, 310, 50);

			_titleText = new UILabel(new System.Drawing.RectangleF(0, 2, 320, 40));
			_titleText.TextAlignment = UITextAlignment.Center;
			_titleText.Font = UIFont.BoldSystemFontOfSize(17);
			_titleText.TextColor = AppStyle.NavigationTitleColor;
            _titleText.BackgroundColor = UIColor.Clear;
			AddSubview(_titleText);

			var image = UIImage.FromFile("Assets/Logo.png");
			_img = new UIImageView(image);
			_img.Frame = new System.Drawing.RectangleF(0, 5, image.Size.Width, image.Size.Height );            
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


		public override System.Drawing.RectangleF Frame {
            get { return base.Frame; }
            set
            { 
                if (_titleText != null & value.X > 0)
                {
                    _titleText.Frame = new System.Drawing.RectangleF(-value.X, _titleText.Frame.Y, _titleText.Frame.Width, _titleText.Frame.Height);
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

