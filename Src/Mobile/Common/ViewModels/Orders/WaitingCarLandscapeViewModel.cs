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

			BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.Subscribe(UpdateModelParametersEvent, СloseWaitingWindowEvent);
		}

		private void UpdateModelParametersEvent(DeviceOrientation deviceOrientation, string carMumber)
		{
			DeviceOrientation = deviceOrientation;
			CarNumber = carMumber;
		}

		private void СloseWaitingWindowEvent()
		{
			BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.UnSubscribe(UpdateModelParametersEvent, СloseWaitingWindowEvent);
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
		public event Action<DeviceOrientation, string> UpdateModelParametersEvent;
		public event Action СloseWaitingWindowEvent;

		public string CarNumber { get; set; }

		public DeviceOrientation DeviceOrientation { get; set; }

		public bool WaitingWindowClosed = false;

		private object _exclusiveAccess = new object();

		public void UpdateModelParameters(DeviceOrientation deviceOrientation, string carMumber)
		{
			lock (_exclusiveAccess)
			{
				if (UpdateModelParametersEvent != null && !WaitingWindowClosed)
				{
					UpdateModelParametersEvent(deviceOrientation, carMumber);
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

		public void Subscribe(Action<DeviceOrientation, string> updateModelParametersEvent, Action closeWaitingWindowEvent)
		{
			UpdateModelParametersEvent += updateModelParametersEvent;
			СloseWaitingWindowEvent += closeWaitingWindowEvent;
		}

		public void UnSubscribe(Action<DeviceOrientation, string> updateModelParametersEvent, Action closeWaitingWindowEvent)
		{
			UpdateModelParametersEvent -= updateModelParametersEvent;
			СloseWaitingWindowEvent -= closeWaitingWindowEvent;
		}
	}
}