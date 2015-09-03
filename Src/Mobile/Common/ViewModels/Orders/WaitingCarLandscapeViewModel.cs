using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;

using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.ViewModels;
using System;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class WaitingCarLandscapeViewModelParameters
	{
		public event Action<DeviceOrientation> UpdateDeviceOrientationEvent;
		public event Action СloseWaitingWindowEvent;

		public string CarNumber { get; set; }

		public DeviceOrientation DeviceOrientation { get; set; }

		public bool WaitingWindowClosed = false;

		public void UpdateDeviceOrientation(DeviceOrientation deviceOrientation)
		{
			lock (this)
			{
				if (UpdateDeviceOrientationEvent != null && !WaitingWindowClosed)
					UpdateDeviceOrientationEvent(deviceOrientation);
			}
		}

		public void CloseWaitingWindow()
		{
			lock (this)
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

	public class WaitingCarLandscapeViewModel:PageViewModel
    {
		string _carNumber;
		DeviceOrientation _deviceOrientation;

		public void Init(WaitingCarLandscapeViewModelParameters waitingCarLandscapeViewModelParameters)
		{
			CarNumber = waitingCarLandscapeViewModelParameters.CarNumber;
			DeviceOrientation = waitingCarLandscapeViewModelParameters.DeviceOrientation;

			BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.Subscribe(UpdateDeviceOrientationEvent, СloseWaitingWindowEvent);
		}

		void UpdateDeviceOrientationEvent(DeviceOrientation deviceOrientation)
		{
			DeviceOrientation = deviceOrientation;
		}

		void СloseWaitingWindowEvent()
		{
			BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.UnSubscribe(UpdateDeviceOrientationEvent, СloseWaitingWindowEvent);
			this.Close(this);
		}

		public string CarNumber
		{
			get
			{
				return _carNumber;
			}
			
			set
			{
				_carNumber = value;
				RaisePropertyChanged();
			}
		}

		public DeviceOrientation DeviceOrientation
		{
			get
			{
				return _deviceOrientation;
			}

			set
			{
				_deviceOrientation = value;
				RaisePropertyChanged();
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
}