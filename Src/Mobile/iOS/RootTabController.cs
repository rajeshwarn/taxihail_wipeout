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
			
			
			this.SetViewControllers (_controllers.ToArray (), false);
			
			this.SelectedViewController = _controllers [0];
			
			
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
				if ((this.SelectedViewController is UINavigationController) && (((UINavigationController)this.SelectedViewController).ViewControllers.Count () > 0) && (((UINavigationController)this.SelectedViewController).ViewControllers [0] is UIViewController))
				{
					return ((UIViewController)((UINavigationController)AppContext.Current.Controller.SelectedViewController).ViewControllers [0]);
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
			var bookTab = _controllers [0].ViewControllers [0] as BookTabView;
			if (data != null)
			{
				bookTab.Rebook (data);
			}
			((UINavigationController)_controllers [0]).ViewControllers.OfType<ISelectableViewController> ().ForEach (c => c.Selected ());
			this.SelectedViewController = _controllers [0];
		}

		private UINavigationController CreateNavigationController<T> (string title, string logo, bool hideNavBar) where T : UIViewController, ITaxiViewController
		{
			
			
			
			UIViewController baseController = Activator.CreateInstance<T> ();
			
			var navController = new TaxiNavigationController ((ITaxiViewController)baseController);
			navController.PushViewController (baseController, false);
			navController.NavigationBar.BarStyle = UIBarStyle.Black;
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
			Console.WriteLine ("RefreshCompanyButtons");
			var id = (AppContext.Current.LoggedUser != null) && (AppContext.Current.LoggedUser.DefaultSettings != null) ? AppContext.Current.LoggedUser.DefaultSettings.Company : -1;
			
			if (CompanyChanged != null)
			{
				CompanyChanged (this, EventArgs.Empty);
			}
			
			foreach (var b in _buttons)
			{
				try
				{
					Console.WriteLine ("RefreshCompanyButtons --");
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
				
				
				
				

				var id = (AppContext.Current.LoggedUser != null) && (AppContext.Current.LoggedUser.DefaultSettings != null) ? AppContext.Current.LoggedUser.DefaultSettings.Company : -1;
				var img = new UIImageView (UIImage.FromFile (AppSettings.GetLogo (id)));

				img.Frame = new System.Drawing.RectangleF (0, 5, 98, 48);
				
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
				_titleText.Text = title;
				_titleText.Font = UIFont.BoldSystemFontOfSize (17);
				_titleText.TextColor = UIColor.Black;
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
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationBar.TintColor = UIColor.FromRGB (255, 178, 14);
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if (!_didAppear)
			{
				_didAppear = true;
				NavigationBar.TintColor = UIColor.FromRGB (255, 178, 14);
					
				NavigationBar.TopItem.TitleView = AppContext.Current.Controller.GetTitleView (_rootViewContrller.GetTopView (), _rootViewContrller.GetTitle ());
				
			}
		}
		
	}
}

