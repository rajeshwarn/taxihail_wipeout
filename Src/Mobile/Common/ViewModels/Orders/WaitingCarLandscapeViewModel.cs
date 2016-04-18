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
	    private readonly IOrientationService _orientationService;
        private string _carNumber;
        private DeviceOrientations _deviceOrientation;

        public WaitingCarLandscapeViewModel(IOrientationService orientationService)
        {
            IsViewVisible = true;
            _orientationService = orientationService;

            _orientationService.ObserveDeviceIsInLandscape()
                .Where(deviceOrientation => deviceOrientation != DeviceOrientation)
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

        public void Init(string carNumber, DeviceOrientations deviceOrientation)
        {
            CarNumber = carNumber;
            DeviceOrientation = deviceOrientation;
        }

	    public static bool IsViewVisible { get; private set; }

        // Overridden to ensure that IsVisible is set to false when closing the page and subscriptions are disposed.
        protected new void Close(IMvxViewModel vm)
        {
            IsViewVisible = false;
            Subscriptions.Clear();

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