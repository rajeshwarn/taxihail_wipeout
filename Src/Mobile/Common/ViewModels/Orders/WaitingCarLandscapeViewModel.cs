using apcurium.MK.Booking.Mobile.AppServices;
using System.Windows.Input;

using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.ViewModels;
using System;

namespace apcurium.MK.Booking.Mobile.ViewModels
{

	public class WaitingCarLandscapeViewModelParameters
	{
		public event Action<DeviceOrientation> Ev1;
		public event Action Ev2;

		public string CarNumber { get; set; }

		public DeviceOrientation DeviceOrientation { get; set; }

		public void Upd(DeviceOrientation dor)
		{
			if (Ev1 != null)
				Ev1(dor);
		}

		public void Cls()
		{
			if (Ev2 != null)
				Ev2();
		}

		public bool recycled = false;
	}

	public class WaitingCarLandscapeViewModel:PageViewModel, ISubViewModel<int>
    {
		string _carNumber;
		DeviceOrientation _deviceOrientation;

		WaitingCarLandscapeViewModelParameters wp;

		public void Init(WaitingCarLandscapeViewModelParameters waitingCarLandscapeViewModelParameters)
		{
			CarNumber = waitingCarLandscapeViewModelParameters.CarNumber;
			DeviceOrientation = waitingCarLandscapeViewModelParameters.DeviceOrientation;

			wp = waitingCarLandscapeViewModelParameters;

			wp.Ev1 += Ev1;
			wp.Ev2 += Ev2;

		}

		void Ev1(DeviceOrientation dor)
		{
			DeviceOrientation = dor;
		}

		void Ev2()
		{
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

		public ICommand ReturnToBookView
		{
			get
			{
				return this.GetCommand(() =>
				{
					wp.recycled = false;
					this.Close(this);
				});
			}
		}
    }
}