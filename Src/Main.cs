
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.Practices.ServiceLocation;
using apcurium.Framework.Extensions;


namespace TaxiMobileApp
{
	public class Application
	{
		static void Main (string[] args)
		{
			try
			{
				UIApplication.Main (args);
				
			}
			catch (Exception ex)
			{
				Logger.LogError (ex);
			}
		}
		
	}

	// The name AppDelegate is referenced in the MainWindow.xib file.
	public partial class AppDelegate : UIApplicationDelegate
	{


		private RootTabController _tabBarController;

		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			
			window.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background_full_nologo.png"));
			
			AppContext.Initialize (window);
			
			Logger.LogMessage ("Context initialized");
			
			_tabBarController = new RootTabController ();
			
			Logger.LogMessage ("MainTabController initialized");
			
			AppContext.Current.Controller = _tabBarController;
			
			new Bootstrapper ( new IModule[] { new AppModule() } ).Run ();
			
			ThreadHelper.ExecuteInThread (() =>
			{
				try
				{
					InvokeOnMainThread (() => _tabBarController.Load ());
					
					BeginInvokeOnMainThread (() =>
					{
						window.AddSubview (_tabBarController.View);
						
						
						
						if (AppContext.Current.LoggedUser == null)
						{
							_tabBarController.SelectedViewController.PresentModalViewController (new LoginView (), true);
						}
						
						
					});
					
					
				}
				catch (Exception ex)
				{
					Logger.LogError (ex);
				}
			});
			
			
			
			
			
			window.MakeKeyAndVisible ();
			
			
			ThreadHelper.ExecuteInThread (() => ServiceLocator.Current.GetInstance<IAccountService> ().EnsureListLoaded ());
			
			
			
			return true;
		}

		// This method is required in iPhoneOS 3.0
		public override void OnActivated (UIApplication application)
		{
			Logger.LogMessage ("OnActivated");
			
			
			
			if (AppContext.Current.LoggedUser != null)
			{
				ThreadHelper.ExecuteInThread (() =>
				{
					try
					{
						
						string error = "";
						
						var service = ServiceLocator.Current.GetInstance<IAccountService> ();
						
						if ((AppContext.Current.Controller.SelectedUIViewController != null) && (AppContext.Current.Controller.SelectedUIViewController is BookTabView))
						{
							AppContext.Current.Controller.View.InvokeOnMainThread (() => { AppContext.Current.Controller.SelectedRefreshableViewController.RefreshData (); });
						}
						
						
						AccountData account = service.GetAccount (AppContext.Current.LoggedUser.Email, AppContext.Current.LoggedUser.Password, out error);
						
						if  ( (account == null) && ( error.HasValue() ) )
						{
							AppContext.Current.SignOutUser();
							InvokeOnMainThread (() => MessageHelper.Show (Resources.InvalidAccountOnActivatedTitle, Resources.InvalidAccountOnActivatedMessage));
							InvokeOnMainThread (() => _tabBarController.SelectedViewController.PresentModalViewController (new LoginView (), true));
							return;
						}
						
						
						
						InvokeOnMainThread (() => AppContext.Current.UpdateLoggedInUser (account, false));
						
						if ((AppContext.Current.Controller.SelectedRefreshableViewController != null) && (!(AppContext.Current.Controller.SelectedUIViewController is BookTabView)))
						{
							AppContext.Current.Controller.View.InvokeOnMainThread (() => { AppContext.Current.Controller.SelectedRefreshableViewController.RefreshData (); });
						}
						
						
						
						
						
						
					}
					catch (Exception ex)
					{
						Logger.LogError (ex);
					}
					finally
					{
						
					}
				});
				
			}

			else
			{
				
				if (  _tabBarController.SelectedViewController != null )
				{
				//	InvokeOnMainThread (() => MessageHelper.Show (Resources.InvalidAccountOnActivatedTitle, Resources.InvalidAccountOnActivatedMessage));
					InvokeOnMainThread (() => _tabBarController.SelectedViewController.PresentModalViewController (new LoginView (), true));
				}
			}
		}

		public override void ReceiveMemoryWarning (UIApplication application)
		{
			AppContext.Current.ReceiveMemoryWarning = true;
			Logger.LogMessage ("ReceiveMemoryWarning");
		}
		
		
	}
	/*
		public override void WillTerminate (UIApplication application)
		{
			//Save data here
		}
		*/	
}

