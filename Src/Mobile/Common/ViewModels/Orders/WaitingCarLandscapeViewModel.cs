using System;
using System.Reactive.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.TaxihailEventArgs;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    public class WaitingCarLandscapeViewModel : PageViewModel
	{
	    private static EventHandler<BookingStatusChangedEventArgs> _bookingStatusChanged;
	    private readonly IDeviceOrientationService _orientationService;
        private string _carNumber;
        private DeviceOrientations _deviceOrientation;

		public static bool IsViewVisible { get; private set; }

        public WaitingCarLandscapeViewModel(IDeviceOrientationService orientationService)
        {
            _orientationService = orientationService;
        }
        public void Init(string carNumber, DeviceOrientations deviceOrientation)
        {
			IsViewVisible = true;
            CarNumber = carNumber;
            DeviceOrientation = deviceOrientation;
        }

		private void HandleBookingStatusChanged(BookingStatusChangedEventArgs args)
		{
			if (args.ShouldCloseWaitingCarLandscapeView)
			{
				CloseCommand.ExecuteIfPossible();
				return;
			}

			CarNumber = args.CarNumber;
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

        public override void OnViewLoaded()
        {
            base.OnViewLoaded();

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
                .Where(args => args.CarNumber != _carNumber || args.ShouldCloseWaitingCarLandscapeView)
                .Subscribe(
                    HandleBookingStatusChanged,
                    Logger.LogError
                )
                .DisposeWith(Subscriptions);
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

		public override void OnViewStopped()
		{
			base.OnViewStopped();
			IsViewVisible = false;
			// Clearing subscriptions to ensure we kill the static event handlers.
			Subscriptions.Clear();
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