using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
using apcurium.Framework.Extensions;


namespace TaxiMobileApp
{
	public class RootTabController : UITabBarController
	{

		private List<UINavigationController> _controllers;


		public RootTabController ()
		{
			
			
		}

		public void Load ()
		{
			
			Logger.StartStopwatch ("loading controllers");
			
			this.ViewControllerSelected += ControllerSelected;
			
			_controllers = new List<UINavigationController> ();
			
			_controllers.Add (CreateNavigationController<BookTabView> (Resources.TabBook, "Assets/Tab/book.png", true));
			
			_controllers.Add (CreateNavigationController<LocationsTabView> (Resources.TabLocations, "Assets/Tab/locations.png", true));
			
			_controllers.Add (CreateNavigationController<HistoryTabView> (Resources.TabHistory, "Assets/Tab/history.png", true));
			
			_controllers.Add (CreateNavigationController<SettingsTabView> (Resources.TabSettings, "Assets/Tab/settings.png", true));
			
			
			this.SetViewControllers (_controllers.ToArray (), true);
			
			this.SelectedViewController = _controllers[0];
			
			
			this.MoreNavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
			
			
			
			Logger.StopStopwatch ("done loading controllers");
			
			
		}

		private void ControllerSelected (UIViewController ctl)
		{
			if (ctl is UINavigationController)
			{
				((UINavigationController)ctl).ViewControllers.OfType<ISelectableViewController> ().ForEach (c => c.Selected ());
				
			}
		}
		void ControllerSelected (object sender, UITabBarSelectionEventArgs e)
		{
			ControllerSelected (e.ViewController);
		}

		public UIViewController SelectedUIViewController {
			get {
				if ((this.SelectedViewController is UINavigationController) && (((UINavigationController)this.SelectedViewController).ViewControllers.Count () > 0) && (((UINavigationController)this.SelectedViewController).ViewControllers[0] is UIViewController))
				{
					return ((UIViewController)((UINavigationController)AppContext.Current.Controller.SelectedViewController).ViewControllers[0]);
				}

				
				else
				{
					return null;
				}
			}
		}

		public IRefreshableViewController SelectedRefreshableViewController {
			get { return SelectedUIViewController as IRefreshableViewController; }
		}


		public void Rebook (BookingInfoData data)
		{
			var bookTab = _controllers[0].ViewControllers[0] as BookTabView;
			if (data != null)
			{
				bookTab.Rebook (data);
			}
			((UINavigationController)_controllers[0]).ViewControllers.OfType<ISelectableViewController> ().ForEach (c => c.Selected ());
			this.SelectedViewController = _controllers[0];
		}

		private UINavigationController CreateNavigationController<T> (string title, string logo, bool hideNavBar) where T : UIViewController, ITaxiViewController
		{
			
			
			
			UIViewController baseController = Activator.CreateInstance<T> ();
			
			var navController = new TaxiNavigationController ((ITaxiViewController)baseController);
			navController.PushViewController (baseController, false);
			navController.NavigationBar.BarStyle = UIBarStyle.Black;
//			navController.NavigationBar.TopItem.Title = "";
//			navController.NavigationBarHidden = false;
//			
//			navController.TopViewController.Title = "";
//			
			using (var image = UIImage.FromFile (logo))
			{
				navController.TabBarItem = new UITabBarItem (title, image, 0);
			}
			
			return navController;
		}



		public UIView GetTitleView (UIView rightView, string title)
		{
			return GetTitleView (rightView, title, false);
		}

		List<UIButton> _buttons = new List<UIButton> ();

		public void RefreshCompanyButtons ()
		{
			Console.WriteLine( "RefreshCompanyButtons" );
			var id = (AppContext.Current.LoggedUser != null) && (AppContext.Current.LoggedUser.DefaultSettings != null) ? AppContext.Current.LoggedUser.DefaultSettings.Company : -1;
			
			if ( CompanyChanged != null )
			{
				CompanyChanged( this, EventArgs.Empty );
			}
			
			foreach (var b in _buttons)
			{
				try
				{
					Console.WriteLine( "RefreshCompanyButtons --" );
					b.SetImage (UIImage.FromFile (AppSettings.GetLogo (id)), UIControlState.Normal);
				}
				catch
				{
				}
			}
		}

		public event EventHandler CompanyChanged;

		public UIView GetTitleView (UIView rightView, string title, bool hideLogo)
		{
			
			var view = new TitleView (new System.Drawing.RectangleF (5, -3, 310, 50));
			if (!hideLogo)
			{
				
				
				//var img = new UIImageView (UIImage.FromFile ( AppSettings.GetLogo( id )));
				
				var img = UIButton.FromType (UIButtonType.Custom);
				_buttons.Add (img);
				var id = (AppContext.Current.LoggedUser != null) && (AppContext.Current.LoggedUser.DefaultSettings != null) ? AppContext.Current.LoggedUser.DefaultSettings.Company : -1;
				img.SetImage (UIImage.FromFile (AppSettings.GetLogo (id)), UIControlState.Normal);
				
				img.TouchUpInside += delegate {
					
					if (AppContext.Current.LoggedUser != null)
					{
						
						var settings = new RideSettingsView (AppContext.Current.LoggedUser.DefaultSettings, true, true);
						var ctl = new UINavigationController (settings);





						settings.Closed += delegate { ThreadHelper.ExecuteInThread (() => { InvokeOnMainThread (() =>{RefreshCompanyButtons ();ctl.DismissModalViewControllerAnimated (true);}); }); };
						
						ctl.NavigationBar.BarStyle = UIBarStyle.Black;
						PresentModalViewController (ctl, true);
					}
				};
				
				img.Frame = new System.Drawing.RectangleF (0, 5, 60, 40);
				
				img.BackgroundColor = UIColor.Clear;
				
				view.AddSubview (img);
			}
			
			if (rightView != null)
			{
				view.AddSubview (rightView);
			}

			
			else
			{
				if (title.HasValue ())
				{
					view.SetTitle (title);
					
				}
			}
			
			return view;
		}
		
		
		
	}

	public class TitleView : UIView
	{
		public TitleView (System.Drawing.RectangleF bound) : base(bound)
		{
			
		}


		//private UITextView _titleText;
		public void SetTitle (string title)
		{
			if (title.HasValue ())
			{
				var _titleText = new UILabel (new System.Drawing.RectangleF (0, 2, 320, 40));
				_titleText.TextAlignment = UITextAlignment.Center;
//				txt.Editable = false;				
//				txt.ScrollEnabled = false;
				_titleText.Text = title;
				_titleText.Font = UIFont.BoldSystemFontOfSize (17);
				_titleText.TextColor = UIColor.White;
				_titleText.BackgroundColor = UIColor.Clear;
				this.AddSubview (_titleText);
			}
		}
		public override System.Drawing.RectangleF Frame {
			get { return base.Frame; }
			set { base.Frame = value; }
		}
	}

	public interface ITaxiViewController
	{

		UIView GetTopView ();

		string GetTitle ();
		
	}
	public class TaxiNavigationController : UINavigationController
	{
		private ITaxiViewController _rootViewContrller;
		public TaxiNavigationController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public TaxiNavigationController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public TaxiNavigationController () : base("TaxiNavigationController", null)
		{
			Initialize ();
		}

		public TaxiNavigationController (ITaxiViewController rootViewContrller)
		{
			_rootViewContrller = rootViewContrller;
			Initialize ();
		}

		void Initialize ()
		{
			
			
			
		}

		private bool _didAppear;

		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (!_didAppear)
			{
				_didAppear = true;
//				var view = new UIView (new System.Drawing.RectangleF (0, 0, UIScreen.MainScreen.Bounds.Width, 50));
//				var img = new UIImageView (UIImage.FromFile ("Assets/TDLogo.png"));
//				img.Frame = new System.Drawing.RectangleF (0, 5, 60, 40);
//				img.BackgroundColor = UIColor.Clear;
//				view.AddSubview (img);
//				
//				var otherView = _rootViewContrller.GetTopView ();
//				if (otherView != null) {
//					view.AddSubview (otherView);
//				} else {
//					var title = _rootViewContrller.GetTitle ();
//					if (title.HasValue ()) {
//						var txt = new UITextView (new System.Drawing.RectangleF (128, 4, 188, 40));
//						txt.TextAlignment = UITextAlignment.Center;
//						txt.Editable = false;
//						txt.ScrollEnabled = false;
//						txt.Text = title;
//						txt.Font = UIFont.BoldSystemFontOfSize (17);
//						txt.TextColor = UIColor.White;
//						txt.BackgroundColor = UIColor.Clear;
//						view.AddSubview (txt);
//					}
//				}
				NavigationBar.TopItem.TitleView = AppContext.Current.Controller.GetTitleView (_rootViewContrller.GetTopView (), _rootViewContrller.GetTitle ());
				
			}
		}
		
	}
}

