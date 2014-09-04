using Android.App;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using apcurium.MK.Common.Configuration;
using Cirrious.CrossCore.Droid.Platform;
using TinyIoC;
using Cirrious.MvvmCross.Platform;
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