using System;
using MonoTouch.UIKit;
using System.Drawing;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using System.ComponentModel;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class DateTimePicker : UIView
	{
		private bool _pickerViewIsShown = false;
		private UIDatePicker _picker;
		private float _pickerViewHeight = 260f;
		private RectangleF _screenBounds;

		public DateTimePicker ( ) :base(   )
		{
			Initialize();
		}

		private void Initialize()
		{
			_screenBounds = UIScreen.MainScreen.Bounds;
			Frame = new RectangleF(0, _screenBounds.Height, _screenBounds.Width, _pickerViewHeight);

			_picker = new UIDatePicker ();
			BackgroundColor = UIColor.Gray;
			
			var accept = AppButtons.CreateStandardButton( new RectangleF (40, 5, 100, 35), Resources.DateTimePickerSetButton, AppStyle.ButtonColor.Silver );
			accept.TouchUpInside += delegate { 
				SetSelectedDate( ((DateTime)_picker.Date).ToLocalTime () );

			};
			AddSubview( accept );
			
			var reset = AppButtons.CreateStandardButton( new RectangleF (_screenBounds.Width - 140, 5, 100, 35), Resources.TimeNow, AppStyle.ButtonColor.Silver );
			reset.TouchUpInside += delegate {
				SetSelectedDate (null);
			};
			AddSubview( reset );
			
			_picker.MinuteInterval = 15;
			_picker.Frame = new System.Drawing.RectangleF (0, 45, _screenBounds.Width, _pickerViewHeight - 40f);
			AddSubview (_picker);
		}

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
					DateChangedCommand.Execute( selectedDate );
				}
			}
		}
	
		public void Show( DateTime? defaultDate = null )
		{
			if( defaultDate.HasValue )
			{
				_picker.SetDate( defaultDate.Value, true );
			}
			else
			{
				_picker.SetDate( DateTime.Now.AddMinutes(15), true );
			}

			if( !ShowPastDate )
			{
				_picker.MinimumDate = DateTime.Now.AddMinutes(15);
			}
			Animate ( true );
		}

		public void Hide()
		{
			Animate(false);
		}

		private void Animate( bool show )
		{
			UIView.Animate( 0.5f, () => {
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
			if( _pickerViewIsShown && !Frame.Contains( this.Superview.ConvertPointFromView( point, this ) ) )
			{
				Hide ();
			}
			return base.PointInside (point, uievent);
		}


	
	}


}

