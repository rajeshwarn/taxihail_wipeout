using System;
using MonoTouch.UIKit;
using System.Drawing;
using Cirrious.MvvmCross.Interfaces.Commands;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class DateTimePicker : UIView
	{
		private bool _pickerViewIsShown = false;
		private UIDatePicker _picker;

		public DateTimePicker ( ) :base( new RectangleF(0, 460, 320, 260)  )
		{
			Initialize();
		}

		private void Initialize()
		{
			_picker = new UIDatePicker ();
			BackgroundColor = UIColor.Gray;
			
			var accept = AppButtons.CreateStandardButton( new RectangleF (40, 5, 100, 35), Resources.Close, AppStyle.ButtonColor.Silver );
			accept.TouchUpInside += delegate { 
				SetSelectedDate( ((DateTime)_picker.Date).ToLocalTime () );
				Animate(false);
			};
			AddSubview( accept );
			
			var reset = AppButtons.CreateStandardButton( new RectangleF (180, 5, 100, 35), Resources.Now, AppStyle.ButtonColor.Silver );
			reset.TouchUpInside += delegate {
				SetSelectedDate (null);
				Animate(false);
			};
			AddSubview( reset );
			
			_picker.MinuteInterval = 15;
			_picker.Frame = new System.Drawing.RectangleF (0, 45, 320, 300);
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
				_picker.SetDate( DateTime.Now, true );
			}

			if( !ShowPastDate )
			{
				_picker.MinimumDate = DateTime.Now;
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
				Frame = new RectangleF( Frame.X, 460 - (show ? Frame.Height : 0 ), Frame.Width, Frame.Height);
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

	
	}


}

