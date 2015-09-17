using System;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Enumeration;

namespace apcurium.MK.Booking.Mobile.ViewModels.Orders
{
	public class WaitingCarLandscapeViewModel : PageViewModel
	{
		private string _carNumber;
		private DeviceOrientations _deviceOrientation;

		public void Init(WaitingCarLandscapeViewModelParameters waitingCarLandscapeViewModelParameters)
		{
			CarNumber = waitingCarLandscapeViewModelParameters.CarNumber;
			DeviceOrientation = waitingCarLandscapeViewModelParameters.DeviceOrientations;

			BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.Subscribe(UpdateModelParametersEvent, СloseWaitingWindowEvent);
		}

		private void UpdateModelParametersEvent(DeviceOrientations deviceOrientations, string carMumber)
		{
			DeviceOrientation = deviceOrientations;
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
						BookingStatusViewModel.WaitingCarLandscapeViewModelParameters.CloseWaitingWindow();
					});
			}
		}
	}

	public class WaitingCarLandscapeViewModelParameters
	{
		public event Action<DeviceOrientations, string> UpdateModelParametersEvent;
		public event Action СloseWaitingWindowEvent;

		public string CarNumber { get; set; }

		public DeviceOrientations DeviceOrientations { get; set; }

		public bool WaitingWindowClosed = false;

		private object _exclusiveAccess = new object();

		public void UpdateModelParameters(DeviceOrientations deviceOrientations, string carMumber)
		{
			lock (_exclusiveAccess)
			{
				if (UpdateModelParametersEvent != null && !WaitingWindowClosed)
				{
					UpdateModelParametersEvent(deviceOrientations, carMumber);
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

		public void Subscribe(Action<DeviceOrientations, string> updateModelParametersEvent, Action closeWaitingWindowEvent)
		{
			UpdateModelParametersEvent += updateModelParametersEvent;
			СloseWaitingWindowEvent += closeWaitingWindowEvent;
		}

		public void UnSubscribe(Action<DeviceOrientations, string> updateModelParametersEvent, Action closeWaitingWindowEvent)
		{
			UpdateModelParametersEvent -= updateModelParametersEvent;
			СloseWaitingWindowEvent -= closeWaitingWindowEvent;
		}
	}
}