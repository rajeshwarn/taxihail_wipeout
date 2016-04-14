using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.EventArgs;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
    //TODO: MKTAXI-4086: This VM must handle it's close always.
    //TODO: MKTAXI-4086: This VM must be notify of orientation changes
    //TODO: MKTAXI-4086: This VM must be notify of Car number changes
    //TODO: MKTAXI-4086: This VM must be notify of OrderStatus wosLoaded
    //TODO: MKTAXI-4086: WARNING, this VM MUST unregister to events when closing.
    public class WaitingCarLandscapeViewModel : PageViewModel
	{
	    private static EventHandler<BookingStatusChangedEventArgs> _bookingStatusChanged;
	    private IOrientationService _orientationService;

        public WaitingCarLandscapeViewModel(IOrientationService orientationService)
        {
            _orientationService = orientationService;
        }

	    public override void OnViewStarted(bool firstTime)
	    {
	        base.OnViewStarted(firstTime);

            

	    }


	    public static bool IsViewVisible { get; private set; }

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