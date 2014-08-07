using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.IoC;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Dialog.Droid;
using Cirrious.MvvmCross.ViewModels;
using TinyIoC;
using apcurium.MK.Booking.MapDataProvider;
using apcurium.MK.Booking.MapDataProvider.Google;
using Cirrious.CrossCore.Droid;
using Cirrious.MvvmCross.Platform;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Droid.Platform;
using Cirrious.MvvmCross.Droid.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
	/// <summary>
	/// /change the way the counter is incr/decr to be able to detect the activated event more closely
	/// Not all event are supported
	/// </summary>
	public class TaxiHailAndroidLifetimeMonitor
		: MvxAndroidLifetimeMonitor
	, IMvxAndroidActivityLifetimeListener
	, IMvxAndroidCurrentTopActivity
	{
		private IAppSettings _settings;
		private IFacebookService _facebookService;
		public TaxiHailAndroidLifetimeMonitor()
		{
						
		}

		private int _createdActivityCount;

		#region IMvxAndroidActivityLifetimeListener Members

		public override void OnCreate(Activity activity)
		{
			Activity = activity;
		}

		public override void OnStart(Activity activity)
		{
			Activity = activity;
			IncrementCounter ();
		}

		public override void OnRestart(Activity activity)
		{
			Activity = activity;
		}

		public override void OnResume(Activity activity)
		{
			TryToFBPublish ();

			Activity = activity;
			IncrementCounter ();		
		}

		protected IAppSettings AppSettings
		{
			get{
				if (_settings == null) {
					_settings = TinyIoCContainer.Current.Resolve<IAppSettings> ();
				}
				return _settings;
			}
		}

		protected IFacebookService FacebookService
		{
			get{
				if (_facebookService == null) {
					_facebookService = TinyIoCContainer.Current.Resolve<IFacebookService> ();
				}
				return _facebookService;
			}
		}


		void TryToFBPublish ()
		{
			try
			{
				if (!string.IsNullOrEmpty (AppSettings.Data.FacebookAppId) && AppSettings.Data.FacebookPublishEnabled) 
				{
					FacebookService.PublishInstall();
				}
			}
			catch
			{

			}
		}

		public override void OnPause(Activity activity)
		{
			DecrementCounter ();
		}

		public override void OnStop(Activity activity)
		{
			DecrementCounter ();
		}

		public override void OnDestroy(Activity activity)
		{
			if (Activity == activity)
				Activity = null;
		}

		private void IncrementCounter()
		{
			_createdActivityCount++;
			if (_createdActivityCount == 1)
			{
				FireLifetimeChange(MvxLifetimeEvent.ActivatedFromMemory);
			}
		}

		private void DecrementCounter()
		{
			_createdActivityCount--;
			if (_createdActivityCount == 0)
			{
				FireLifetimeChange(MvxLifetimeEvent.Closing);
			}
		}

		public override void OnViewNewIntent(Activity activity)
		{
			Activity = activity;
		}

		#endregion

		#region IMvxAndroidCurrentTopActivity Members

		public Activity Activity { get; private set; }

		#endregion
	}

}