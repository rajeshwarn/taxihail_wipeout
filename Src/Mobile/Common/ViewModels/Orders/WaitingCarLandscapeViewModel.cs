using System;
using System.Reactive.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.TaxihailEventArgs;
using apcurium.MK.Common.Enumeration;
using Cirrious.MvvmCross.ViewModels;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    public class WaitingCarLandscapeViewModel : PageViewModel
	{
	    private static EventHandler<BookingStatusChangedEventArgs> _bookingStatusChanged;
	    private IOrientationService _orientationService;

        public WaitingCarLandscapeViewModel(IOrientationService orientationService)
        {
            _orientationService = orientationService;

            _orientationService.ObserveDeviceIsInLandscape()
                .Subscribe(orientation =>
                {
                    DeviceOrientation = orientation;
                },
                Logger.LogError)
                .DisposeWith(Subscriptions);

            Observable.FromEventPattern<EventHandler<BookingStatusChangedEventArgs>, BookingStatusChangedEventArgs>(
                h => _bookingStatusChanged += h,
                h => _bookingStatusChanged -= h
                )
                .Select(args => args.EventArgs)
                .Do(args =>
                {
                    if (args.ShouldCloseWaitingCarLandscapeView)
                    {
                        Close(this);
                        return;
                    }

                    CarNumber = args.CarNumber;
                })
                .Subscribe(
                    _ => { },
                    Logger.LogError
                )
                .DisposeWith(Subscriptions);
        }

        public void Init(string carNumber, DeviceOrientations deviceOrientations)
        {
            CarNumber = carNumber;
            DeviceOrientation = deviceOrientations;
        }

	    public override void OnViewStarted(bool firstTime)
	    {
	        base.OnViewStarted(firstTime);

	        IsViewVisible = true;
	    }

	    public static bool IsViewVisible { get; private set; }

        protected new void Close(IMvxViewModel vm)
        {
            IsViewVisible = false;

            base.Close(vm);
        }

	    public static void NotifyBookingStatusChanged(object sender, string carNumber, bool shouldClose)
	    {
	        if (_bookingStatusChanged != null)
	        {
	            _bookingStatusChanged(sender, new BookingStatusChangedEventArgs()
	            {
	                CarNumber = carNumber,
                    ShouldCloseWaitingCarLandscapeView = shouldClose
	            });
	        }
	    }

        
		private string _carNumber;
		private DeviceOrientations _deviceOrientation;

	    

	    public void Init(string carNumber, int deviceOrientations)
		{
			CarNumber = carNumber;
			DeviceOrientation = (DeviceOrientations)deviceOrientations;
		}

		public string CarNumber
		{
			get { return _carNumber; }
			set
			{
				if (_carNumber != value)
				{
					_carNumber = value;
					RaisePropertyChanged();
				}
			}
		}

		public DeviceOrientations DeviceOrientation
		{
			get { return _deviceOrientation; }
			set
			{
				if (_deviceOrientation != value)
				{
					_deviceOrientation = value;
					RaisePropertyChanged();
				}
			}
		}

		public ICommand CloseView
		{
			get
			{
				return this.GetCommand(() =>
				{
				    Close(this);
				});
			}
		}
	}
}