using System;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class DateTimePicker : UIView
	{
		private bool _pickerViewIsShown;
		private UIDatePicker _picker;
		private float _pickerViewHeight = 260f;
		private RectangleF _screenBounds;
        private GradientButton _accept;
        private UIView _acceptDisableOverlay;
		public DateTimePicker ( string cultureInfo )
		{
			Initialize(cultureInfo);
		}

        private void Initialize(string cultureInfo )
		{
			_screenBounds = UIScreen.MainScreen.Bounds;
			Frame = new RectangleF(0, _screenBounds.Height, _screenBounds.Width, _pickerViewHeight);

			_picker = new UIDatePicker ();
            _picker.MinimumDate = DateTime.Now.AddMinutes(10 );
			BackgroundColor = UIColor.Gray;

            _accept = AppButtons.CreateStandardButton(new RectangleF(10, 5, 250, 35), Localize.GetValue("DateTimePickerSetButton"), AppStyle.ButtonColor.Green);
            _acceptDisableOverlay = new UIView{ BackgroundColor = UIColor.Black , Alpha = 0.5f };
            _acceptDisableOverlay.Frame = new RectangleF (10, 5, 250, 35);
             
            _accept.TouchUpInside += delegate { 
				SetSelectedDate( ((DateTime)_picker.Date).ToLocalTime () );

			};
            AddSubview( _accept );
            AddSubview (_acceptDisableOverlay );
            var reset = AppButtons.CreateStandardButton( new RectangleF (_screenBounds.Width - 50, 5, 40, 35), "", AppStyle.ButtonColor.Red, "Assets/cancel.png" );
			reset.TouchUpInside += delegate {
				SetSelectedDate (null);
			};
			AddSubview( reset );
			
            _picker.Locale = new NSLocale( cultureInfo );
			_picker.MinuteInterval = 5;
			_picker.Frame = new RectangleF (0, 45, _screenBounds.Width, _pickerViewHeight - 40f);
            _picker.ValueChanged += HandleValueChanged;

            _accept.Enabled = false;

			AddSubview (_picker);
		}

        void HandleValueChanged (object sender, EventArgs e)
        {
            _accept.Enabled = ((DateTime)_picker.Date).ToLocalTime ()  > DateTime.Now.AddMinutes (15);
         
            if ( ((DateTime)_picker.Date).ToLocalTime ()  > DateTime.Now.AddMinutes (15) )
            {
                _acceptDisableOverlay.RemoveFromSuperview ();
            }
            else
            {
                AddSubview (_acceptDisableOverlay );
            }
        }

// ReSharper disable once MemberCanBePrivate.Global
		public bool ShowPastDate { get;set;}

		private void SetSelectedDate( DateTime? selectedDate )
		{
			if( DateChangedCommand != null )
			{
				if( selectedDate.HasValue )
				{
					DateChangedCommand.Execute( ((DateTime)selectedDate).ToLocalTime() );
				}
				else
				{
					DateChangedCommand.Execute(null);
				}
			}
		}
	
		public void Show( DateTime? defaultDate = null )
		{
			if( defaultDate.HasValue )
			{
                _picker.SetDate( defaultDate.Value.AddMinutes(15), true );
			}
			else
			{
				_picker.SetDate( DateTime.Now.AddMinutes(15), true );
			}

            HandleValueChanged( this, EventArgs.Empty );
			if( !ShowPastDate )
			{
				_picker.MinimumDate = DateTime.Now.AddMinutes(10 );
			}
			Animate ( true );
		}

		public void Hide()
		{
			Animate(false);
		}

		private void Animate( bool show )
		{
			Animate( 0.5f, () => {
				Frame = new RectangleF( Frame.X, _screenBounds.Height - (show ? Frame.Height : 0 ), Frame.Width, Frame.Height);
			});
			_pickerViewIsShown = !_pickerViewIsShown;
			
		}

		private IMvxCommand _dateChangedCommand;
		public IMvxCommand DateChangedCommand
		{ 
			get{ return _dateChangedCommand;}
			set
			{
				_dateChangedCommand = value;
			}
		}

		public override bool PointInside (PointF point, UIEvent uievent)
		{
			if( _pickerViewIsShown && !Frame.Contains( Superview.ConvertPointFromView( point, this ) ) )
			{
				Hide ();
			}
			return base.PointInside (point, uievent);
		}


	
	}


}

