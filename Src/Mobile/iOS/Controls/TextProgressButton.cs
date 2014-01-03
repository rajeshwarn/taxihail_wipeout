using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	[Register ("TextProgressButton")]
	public class TextProgressButton : UIControl
    {
		private UILabel _line1Label;
		private UILabel _line2Label;
		private UIImageView _imageView;
		private UIActivityIndicatorView _progress;

		public TextProgressButton(IntPtr handle) : base(  handle )
        {
			Initialize();
        }
        
		
		private void Initialize()
		{
			var image = UIImage.FromFile( "Assets/right_arrow.png" );

			_line1Label = new UILabel( new RectangleF( 8, 4, Frame.Width - image.Size.Width - 10, 20 ) );
			_line1Label.BackgroundColor = UIColor.Clear;
			_line1Label.TextColor = AppStyle.DarkText;
			_line1Label.Font = AppStyle.GetNormalFont( 19 );
			_line1Label.TextAlignment = UITextAlignment.Left;
			AddSubview( _line1Label );

			_line2Label = new UILabel( new RectangleF( 8, 24, Frame.Width - image.Size.Width - 10, 20 ) );
			_line2Label.BackgroundColor = UIColor.Clear;
			_line2Label.TextColor = AppStyle.GreyText;
            _line2Label.Font = AppStyle.GetNormalFont( 13 );
			_line2Label.TextAlignment = UITextAlignment.Left;
			AddSubview( _line2Label );


			_imageView = new UIImageView( new RectangleF( _line1Label.Frame.Width + 5, Frame.Height/2 - image.Size.Height/2, image.Size.Width, image.Size.Height ) );
			_imageView.BackgroundColor = UIColor.Clear;
			_imageView.Image = image;
			_imageView.Hidden = true;
			AddSubview( _imageView );

			_progress = new UIActivityIndicatorView( new RectangleF( _line1Label.Frame.Width -5, 0, image.Size.Width, Frame.Height ) );
			_progress.BackgroundColor = UIColor.Clear;
			_progress.Hidden = true;
			_progress.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray;
			AddSubview( _progress );

            TouchDown += HandleTouchDown;
            TouchUpInside += HandleTouchUp;
            TouchUpOutside += HandleTouchUp;
           
		}

        void HandleTouchUp (object sender, EventArgs e)
        {
            BackgroundColor = UIColor.Clear;
        }
        
        void HandleTouchDown (object sender, EventArgs e)
        {
            BackgroundColor = UIColor.FromRGBA(0,0,0,50);
        }

        public string TextLine1
        {
            get { return _line1Label.Text;}
            set { _line1Label.Text = value;}
        }

        public string TextLine2
        {
			get { return _line2Label.Text;}
			set { _line2Label.Text = value;}
        }

		private bool _isSearching;
		public bool IsSearching {
			get { return _isSearching; }
			set{ _isSearching = value;
				RefreshUI();
			}
		}

		private bool _isPlaceholder;
		public bool IsPlaceholder {
			get{ return _isPlaceholder; }
			set{ _isPlaceholder = value; 
				RefreshUI();
			}
		}

		private void RefreshUI( )
		{
			_imageView.Hidden = _isSearching;
			_progress.Hidden = !_isSearching;
			if( _isSearching )
			{
				_progress.StartAnimating();
			}
			else
			{
				_progress.StopAnimating();
			}

            var image = _imageView.Image;
            if ( image != null )
            {
            _line1Label.Frame = IsSearching || _isPlaceholder ?new RectangleF( 8, 12, Frame.Width - image.Size.Width - 10, 20 ) : new RectangleF( 8, 4, Frame.Width - image.Size.Width - 10, 20 );
            _line1Label.Font = IsSearching || _isPlaceholder ? AppStyle.GetItalicFont( 15 ) : AppStyle.GetNormalFont( 19 );
            }
		}

	}
}


  