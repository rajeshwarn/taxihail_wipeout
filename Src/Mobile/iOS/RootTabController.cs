using System;
using System.Collections.Generic;
using System.Linq;

using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreFoundation;
using apcurium.Framework.Extensions;
using apcurium.MK.Booking.Api.Contract.Resources;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RootTabController : UITabBarController
    {

        private List<UINavigationController> _controllers;

        public RootTabController()
        {
            
            
        }

        public void Load()
        {
            
            Logger.StartStopwatch("loading controllers");
            
            this.ViewControllerSelected += ControllerSelected;
            
            _controllers = new List<UINavigationController>();
            
            _controllers.Add(CreateNavigationController<BookTabView>(Resources.TabBook, "Assets/Tab/book.png", true));
            
            _controllers.Add(CreateNavigationController<LocationsTabView>(Resources.TabLocations, "Assets/Tab/locations.png", true));
            
            _controllers.Add(CreateNavigationController<HistoryTabView>(Resources.TabHistory, "Assets/Tab/history.png", true));
            
            _controllers.Add(CreateNavigationController<SettingsTabView>(Resources.TabSettings, "Assets/Tab/settings.png", true));
            
            
            this.SetViewControllers(_controllers.ToArray(), false);
            
            this.SelectedViewController = _controllers[0];
            
            
            this.MoreNavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
            
            
            
            Logger.StopStopwatch("done loading controllers");
            
            
        }

        private void ControllerSelected(UIViewController ctl)
        {
            if (ctl is UINavigationController)
            {
                ((UINavigationController)ctl).ViewControllers.OfType<ISelectableViewController>().ForEach(c => c.Selected());
                
            }
        }

        void ControllerSelected(object sender, UITabBarSelectionEventArgs e)
        {
            ControllerSelected(e.ViewController);
        }

        public UIViewController SelectedUIViewController
        {
            get
            {
                if ((this.SelectedViewController is UINavigationController) && (((UINavigationController)this.SelectedViewController).ViewControllers.Count() > 0) && (((UINavigationController)this.SelectedViewController).ViewControllers[0] is UIViewController))
                {
                    return ((UIViewController)((UINavigationController)AppContext.Current.Controller.SelectedViewController).ViewControllers[0]);
                }
                else
                {
                    return null;
                }
            }
        }

        public IRefreshableViewController SelectedRefreshableViewController
        {
            get { return SelectedUIViewController as IRefreshableViewController; }
        }

        public void Rebook(Order data)
        {
            var bookTab = _controllers[0].ViewControllers[0] as BookTabView;
            if (data != null)
            {
                bookTab.Rebook(data);
            }
            ((UINavigationController)_controllers[0]).ViewControllers.OfType<ISelectableViewController>().ForEach(c => c.Selected());
            this.SelectedViewController = _controllers[0];
        }

        private UINavigationController CreateNavigationController<T>(string title, string logo, bool hideNavBar) where T : UIViewController, ITaxiViewController
        {
            
            
            
            UIViewController baseController = Activator.CreateInstance<T>();
            
            var navController = new TaxiNavigationController((ITaxiViewController)baseController);
            navController.PushViewController(baseController, false);
            navController.NavigationBar.BarStyle = UIBarStyle.Black;
            using (var image = UIImage.FromFile (logo))
            {
                navController.TabBarItem = new UITabBarItem(title, image, 0);
            }
            
            return navController;
        }

        public UIView GetTitleView(UIView rightView, string title)
        {
            return GetTitleView(rightView, title, false);
        }

        public event EventHandler CompanyChanged;

        public UIView GetTitleView(UIView rightView, string title, bool hideLogo)
        {
            
            var view = new TitleView(new System.Drawing.RectangleF(5, -3, 310, 50));
            if (!hideLogo)
            {                               
                var img = new UIImageView(UIImage.FromFile("Assets/Logo.png"));

                img.Frame = new System.Drawing.RectangleF(0, 5, 72, 49);
                
                img.BackgroundColor = UIColor.Clear;
                
                view.AddSubview(img);
            }
            
            if (rightView != null)
            {
                view.AddSubview(rightView);
            }
            else
            {
                if (title.HasValue())
                {
                    view.SetTitle(title);
                    
                }
            }
            
            return view;
        }
        
        
        
    }

    public class TitleView : UIView
    {
        private  UILabel _titleText;

        public TitleView(System.Drawing.RectangleF bound) : base(bound)
        {
            AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleRightMargin | UIViewAutoresizing.FlexibleLeftMargin;
            AutosizesSubviews = false;
        }
                
        public override System.Drawing.RectangleF Frame
        {
            get { return base.Frame; }
            set
            { 
                if (_titleText != null & value.X > 0)
                {
                    _titleText.Frame = new System.Drawing.RectangleF(-value.X, _titleText.Frame.Y, _titleText.Frame.Width, _titleText.Frame.Height);
                }
                base.Frame = value;
            }
        }

        //private UITextView _titleText;
        public void SetTitle(string title)
        {
            if (title.HasValue())
            {
                _titleText = new UILabel(new System.Drawing.RectangleF(0, 2, 320, 40));
                _titleText.TextAlignment = UITextAlignment.Center;
                _titleText.Text = title;
                _titleText.Font = UIFont.BoldSystemFontOfSize(17);
                _titleText.TextColor = UIColor.White;
                _titleText.BackgroundColor = UIColor.Clear;
                this.AddSubview(_titleText);
            }
        }


     
    }

    public interface ITaxiViewController
    {

        UIView GetTopView();

        string GetTitle();
        
    }

    public class TaxiNavigationController : UINavigationController
    {
        private ITaxiViewController _rootViewContrller;

        public TaxiNavigationController(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public TaxiNavigationController(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public TaxiNavigationController() : base("TaxiNavigationController", null)
        {
            Initialize();
        }

        public TaxiNavigationController(ITaxiViewController rootViewContrller)
        {
            _rootViewContrller = rootViewContrller;
            Initialize();
        }

        void Initialize()
        {
            
            
            
        }

        private bool _didAppear;
        
        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
            LoadBackgroundNavBar();
        }

        private void LoadBackgroundNavBar()
        {
            NavigationBar.TintColor = UIColor.FromRGB(0, 78, 145);

            //It might crash on iOS version smaller than 5.0
            try
            {
                NavigationBar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
            catch
            {
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            if (!_didAppear)
            {
                _didAppear = true;
                
                LoadBackgroundNavBar();              


                NavigationBar.TopItem.TitleView = AppContext.Current.Controller.GetTitleView(_rootViewContrller.GetTopView(), _rootViewContrller.GetTitle());
                
            }
        }
        
    }
}

