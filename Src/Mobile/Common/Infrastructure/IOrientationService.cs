using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace apcurium.MK.Booking.Mobile.AppServices
{
	public enum DeviceOrientation
	{
		Up,
		Down,
		Left,
		Right
	}

	public interface IOrientationService
	{
		void Initialize(DeviceOrientation[] deviceOrientationNotifications);

		bool IsAvailable();

		bool Start();

		bool Stop();

		void SubscribeToOrientationChange(Action<DeviceOrientation> eventHandler);

		void UnSubscribeToOrientationChange(Action<DeviceOrientation> eventHandler);

		void SubscribeToAngleChange(Action<int> eventHandler);

		void UnSubscribeToAngleChange(Action<int> eventHandler);
	}
}