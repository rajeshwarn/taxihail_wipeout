using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Cirrious.MvvmCross.Binding.Bindings.Target.Construction;
using Cirrious.MvvmCross.Dialog.Droid;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices.Social;
using apcurium.MK.Booking.Mobile.AppServices.Social.OAuth;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Settings;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Activities.Book;
using apcurium.MK.Booking.Mobile.Client.Binding;
using apcurium.MK.Booking.Mobile.Client.Cache;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Converters;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.PlatformIntegration;
using apcurium.MK.Booking.Mobile.Client.Services.Social;
using Cirrious.MvvmCross.ViewModels;
using apcurium.MK.Booking.Mobile.IoC;
using Cirrious.CrossCore.Droid.Platform;
using Java.IO;
using System.Threading;
using System.Collections.Concurrent;
using Cirrious.CrossCore;
using System.IO;
using System.Linq;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class Setup
        : MvxAndroidDialogSetup
    {
        readonly TinyIoCContainer _container;

		public Setup(Context applicationContext) : base(applicationContext)
        {
            _container = TinyIoCContainer.Current;
        }

		protected override IMvxApplication CreateApp()
		{
			return new TaxiHailApp();
		}

		protected override List<Type> ValueConverterHolders
		{
			get
			{
				return new List<Type> { typeof(AppConverters) };
			}
		}

		protected override void InitializeLastChance()
        {
			base.InitializeLastChance();

            _container.Register<IPackageInfo>(new PackageInfo(ApplicationContext));
            _container.Register<ILogger, LoggerImpl>();
            _container.Register<IMessageService>(new MessageService(ApplicationContext));
            _container.Register<IAnalyticsService>((c, x) => new GoogleAnalyticsService(Application.Context, c.Resolve<IPackageInfo>(), c.Resolve<IAppSettings>(), c.Resolve<ILogger>()));

            _container.Register<ILocationService>(new LocationService());

			_container.Register<ILocalization>(new Localize(ApplicationContext,_container.Resolve<ILogger>()));
            _container.Register<ICacheService>(new CacheService());
            _container.Register<ICacheService>(new CacheService("MK.Booking.Application.Cache"), "AppCache");
            _container.Register<IPhoneService>(new PhoneService(ApplicationContext));
            _container.Register<IPushNotificationService>((c, p) => new PushNotificationService(ApplicationContext, c.Resolve<IAppSettings>()));

            _container.Register<IAppSettings>(new AppSettingsService(_container.Resolve<ICacheService>(), _container.Resolve<ILogger>()));

			CpuBenchmark ();
			InitializeSocialNetwork();
        }

		protected override void FillTargetFactories(IMvxTargetBindingFactoryRegistry registry)
        {
            base.FillTargetFactories(registry);
            registry.RegisterFactory(new MvxCustomBindingFactory<EditTextRightSpinner>("SelectedItem", spinner => new EditTextRightSpinnerSelectedItemBinding(spinner)));
            registry.RegisterFactory(new MvxCustomBindingFactory<EditTextSpinner>("SelectedItem", spinner => new EditTextSpinnerSelectedItemBinding(spinner)));
			registry.RegisterFactory(new MvxCustomBindingFactory<EditTextLeftImage>("CreditCardNumber", editTextLeftImage => new EditTextCreditCardNumberBinding(editTextLeftImage)));
			registry.RegisterFactory(new MvxCustomBindingFactory<ListViewCell>("IsBottom", cell => new CellItemBinding(cell, CellItemBindingProperty.IsBottom)));
			registry.RegisterFactory(new MvxCustomBindingFactory<ListViewCell>("IsTop", cell => new CellItemBinding(cell, CellItemBindingProperty.IsTop)));
            CustomBindingsLoader.Load(registry);
        }

		private void InitializeSocialNetwork()
		{
			
            _container.Register<IFacebookService>((c,prop) =>  {

                var settings = c.Resolve<IAppSettings>();
                var facebookService = new FacebookService(settings.Data.FacebookAppId, () => _container.Resolve<IMvxAndroidCurrentTopActivity>().Activity);
                return facebookService;
            });			

            _container.Register<ITwitterService>((c,p) => {

                var settings = c.Resolve<IAppSettings>();
                var oauthConfig = new OAuthConfig
                {
                    ConsumerKey = settings.Data.TwitterConsumerKey,
                    Callback = settings.Data.TwitterCallback,
                    ConsumerSecret = settings.Data.TwitterConsumerSecret,
                    RequestTokenUrl = settings.Data.TwitterRequestTokenUrl,
                    AccessTokenUrl = settings.Data.TwitterAccessTokenUrl,
                    AuthorizeUrl = settings.Data.TwitterAuthorizeUrl
                };
                return new TwitterServiceMonoDroid( oauthConfig, c.Resolve<IMvxAndroidCurrentTopActivity>());
            
            });
		}

		protected override Cirrious.CrossCore.IoC.IMvxIoCProvider CreateIocProvider()
		{
			return new TinyIoCProvider(TinyIoCContainer.Current);
		}

        protected override Cirrious.MvvmCross.Droid.Views.IMvxAndroidViewPresenter CreateViewPresenter()
        {
            return new PhonePresenter();
        }



		// Debug CPU
		private void CpuBenchmark()
		{
			System.Timers.Timer timer = new System.Timers.Timer ();
			timer.Interval = 250;
			timer.AutoReset = true;
			timer.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) => {
				readUsage();
			};
			timer.Start ();
		}

		private float[] cpuStack = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f };
		private int cpuStackPointer = 0;

		private void readUsage() {
			try {
				var topActivity = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxAndroidCurrentTopActivity> (); 
				RandomAccessFile reader = new RandomAccessFile ("/proc/stat", "r");
				string load = reader.ReadLine ();

				string[] toks = load.Split (' ');

				long idle1 = long.Parse (toks [5]);
				long cpu1 = long.Parse (toks [2]) + long.Parse (toks [3]) + long.Parse (toks [4])
				            + long.Parse (toks [6]) + long.Parse (toks [7]) + long.Parse (toks [8]);

				try {
					Thread.Sleep (360);
				} catch (Exception) {
				}

				reader.Seek (0);
				load = reader.ReadLine ();
				reader.Close ();

				toks = load.Split (' ');

				float idle2 = float.Parse (toks [5]);
				float cpu2 = float.Parse (toks [2]) + float.Parse (toks [3]) + float.Parse (toks [4])
				             + float.Parse (toks [6]) + float.Parse (toks [7]) + float.Parse (toks [8]);

				float cpuValue = ((cpu2 - cpu1) * 100f / ((cpu2 + idle2) - (cpu1 + idle1)));
				cpuStack [cpuStackPointer++] = cpuValue;

				if (cpuStackPointer == 10) {
					cpuStackPointer = 0;
				}
				var averageTxt = ((int)cpuStack.Take(10).Average()).ToString ().PadLeft(2,'0');

				TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));

				//Get VM Heap Size by calling:
				var heapSize = (Java.Lang.Runtime.GetRuntime().TotalMemory() / 1000).ToString().PadLeft(6,'0');
				//Get Allocated VM Memory by calling:
				var allocated = ((Java.Lang.Runtime.GetRuntime().TotalMemory() - Java.Lang.Runtime.GetRuntime().FreeMemory()) / 1000).ToString().PadLeft(6,'0');
				// Free
				var free = (Java.Lang.Runtime.GetRuntime().FreeMemory() / 1000).ToString().PadLeft(6,'0');
				//Get VM Heap Size Limit by calling:
				var heapSizeLimit = (Java.Lang.Runtime.GetRuntime().MaxMemory() / 1000).ToString().PadLeft(6,'0');
				//
				var mem = heapSize+" " + allocated + " " + free + " " + heapSizeLimit;
				System.Console.WriteLine ("-|-cpu " + (((long)t.TotalSeconds * 1000L) + (long)DateTime.Now.Millisecond).ToString() + " " + averageTxt + " " + mem + " " + topActivity.Activity.Title);


			} catch (System.IO.IOException) {
				//ex.printStackTrace();
			}
		}
	}
}