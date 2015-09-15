using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.ViewModels;
using System;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class WaitingCarLandscapeViewModel : PageViewModel
	{
		private string _carNumber;
		private DeviceOrientation _deviceOrientation;

		public void Init(WaitingCarLandscapeViewModelParameters waitingCarLandscapeViewModelParameters)
		{
			CarNumber = waitingCarLandscapeViewModelParameters.CarNumber;
			DeviceOrientation = waitingCarLandscapeViewModelParameters.DeviceOrientation;

			BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.Subscribe(UpdateDeviceOrientationEvent, СloseWaitingWindowEvent);
		}

		private void UpdateDeviceOrientationEvent(DeviceOrientation deviceOrientation)
		{
			DeviceOrientation = deviceOrientation;
		}

		private void СloseWaitingWindowEvent()
		{
			BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.UnSubscribe(UpdateDeviceOrientationEvent, СloseWaitingWindowEvent);
			Close(this);
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

		public DeviceOrientation DeviceOrientation
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
						BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.CloseWaitingWindow();
					});
			}
		}
	}

	public class WaitingCarLandscapeViewModelParameters
	{
		public event Action<DeviceOrientation> UpdateDeviceOrientationEvent;
		public event Action СloseWaitingWindowEvent;

		public string CarNumber { get; set; }

		public DeviceOrientation DeviceOrientation { get; set; }

		public bool WaitingWindowClosed = false;

		private object _exclusiveAccess = new object();

		public void UpdateDeviceOrientation(DeviceOrientation deviceOrientation)
		{
			lock (_exclusiveAccess)
			{
				if (UpdateDeviceOrientationEvent != null && !WaitingWindowClosed)
				{
					UpdateDeviceOrientationEvent(deviceOrientation);
				}
			}
		}

		public void CloseWaitingWindow()
		{
			lock (_exclusiveAccess)
			{
				if (СloseWaitingWindowEvent != null && !WaitingWindowClosed)
				{
					СloseWaitingWindowEvent();
					WaitingWindowClosed = true;
				}
			}
		}

		public void Subscribe(Action<DeviceOrientation> updateDeviceOrientationEvent, Action closeWaitingWindowEvent)
		{
			UpdateDeviceOrientationEvent += updateDeviceOrientationEvent;
			СloseWaitingWindowEvent += closeWaitingWindowEvent;
		}

		public void UnSubscribe(Action<DeviceOrientation> updateDeviceOrientationEvent, Action closeWaitingWindowEvent)
		{
			UpdateDeviceOrientationEvent -= updateDeviceOrientationEvent;
			СloseWaitingWindowEvent -= closeWaitingWindowEvent;
		}
	}
}