using System;
using Microsoft.Practices.ServiceLocation;
using SocialNetworks.Services.MonoTouch;
using SocialNetworks.Services;
using TaxiMobileApp.Lib;

namespace TaxiMobileApp
{
	public class AppModule : IModule 
	{
		public AppModule ()
		{
		}

		#region IModule implementation
		public void Initialize ()
		{
			ServiceLocator.Current.Register<IAppResource, Resources>();
			ServiceLocator.Current.RegisterSingleInstance2<IFacebookService>(new FacebookServiceMT( new SocialNetworksService().FacebookAppId ) );
			ServiceLocator.Current.RegisterFactory<ITwitterService>( () => new TwitterServiceMonoTouch( new SocialNetworksService().GetOAuthConfig(), AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController != null ? AppContext.Current.Window.RootViewController.PresentedViewController.ModalViewController : AppContext.Current.Window.RootViewController.PresentedViewController ) );
		}
		#endregion
	}
}

