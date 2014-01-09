using System.Drawing;
using System.Linq;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.Client.Navigation;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter: MvxModalSupportTouchViewPresenter
    {
        private UINavigationController _masterNavigationController;

        public PhonePresenter(UIApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
        {
        }

        protected override UINavigationController CreateNavigationController(UIViewController viewController)
        {
            _masterNavigationController = base.CreateNavigationController(viewController);
            AppContext.Current.Controller = _masterNavigationController;
            _masterNavigationController.NavigationBarHidden = false;


            LoadFakeNavigationBackground();
            LoadBackgroundNavBar(_masterNavigationController.NavigationBar);
            return _masterNavigationController;
        }
        //Sometime we cannot show the navigation bar.  We need to load the fake background to avoid seeing a white background on top and at bottom
        private void LoadFakeNavigationBackground()
        {
            var navBar = new UIImageView();
            navBar.Image = UIImage.FromFile("Assets/navBar.png");
            navBar.Frame = new RectangleF(0, 20, 320, 44);
            _masterNavigationController.View.InsertSubview(navBar, 0);

            var back = new UIImageView();
            back.Image = UIImage.FromFile("Assets/background.png");
            back.Frame = new RectangleF(0, 64, 320, back.Image.Size.Height);
            _masterNavigationController.View.InsertSubview(back, 0);

        }

        public override void Show(IMvxTouchView view)
        {        
           
            
            if (view is IMvxModalTouchView)
            {
                PresentModalViewController(view as UIViewController, true);
            }
            else
            {
                base.Show(view); 
                var navigationController = ((UIViewController)view).NavigationController;
                if (navigationController != null)
                {
                    navigationController.NavigationBar.Hidden = HideNavBar(view);
                }
            }
        }

        private bool HideNavBar(IMvxTouchView view)
        {
            return (view is INavigationView) && (((INavigationView)view).HideNavigationBar);
        }

        private UIViewController _modal;

        public override bool PresentModalViewController(UIViewController viewController, bool animated)
        {

            if (CurrentTopViewController != null)
            {

                viewController.View.Frame = new RectangleF(0, 0, CurrentTopViewController.View.Bounds.Width, CurrentTopViewController.View.Bounds.Height);
                _modal = viewController;
                CurrentTopViewController.View.AddSubview(viewController.View);
                return true;
            }
            return false;
        }

        public override void Close(IMvxViewModel toClose)
        {
            if (_modal != null)
            {
                _modal.View.RemoveFromSuperview();
                _modal = null;
            }
            else
            {
                var topViewController = _masterNavigationController.TopViewController;

                if (topViewController == null)
                {
                    MvxTrace.Trace(MvxTraceLevel.Warning, "Don't know how to close this viewmodel - no topmost");
                    return;
                }

                var topView = topViewController as IMvxTouchView;
                if (topView == null)
                {
                    MvxTrace.Trace(MvxTraceLevel.Warning, "Don't know how to close this viewmodel - topmost is not a touchview");
                    return;
                }

                var viewModel = topView.ReflectionGetViewModel();
                if (viewModel != toClose)
                {
                    var viewControllers = _masterNavigationController.ViewControllers.ToList();
                    var toCloseController = (UIViewController)viewControllers.OfType<IHaveViewModel>().FirstOrDefault(vc => vc.MyViewModel == toClose);


                    if (toCloseController == null)
                    {
                        //todo should throw
                        MvxTrace.Trace(MvxTraceLevel.Warning,
                            "Don't know how to close this viewmodel - topmost view does not present this viewmodel - try inheriting from IHaveViewModel");
                    }
                    else
                    {
                        viewControllers.Remove(toCloseController);
                        _masterNavigationController.ViewControllers = viewControllers.ToArray();
                    }
                    return;
                }

                if (_masterNavigationController.ViewControllers.Length >= 2)
                {

                    var typeController = _masterNavigationController.ViewControllers[1].GetType();
                    if (typeController.IsDefined(typeof(NoHistoryAttribute), false))
                    {
                        var newStack = _masterNavigationController.ViewControllers.ToList();
                        newStack.RemoveAt(1);
                        _masterNavigationController.SetViewControllers(newStack.ToArray(), true);
                    }
                }
                _masterNavigationController.PopViewControllerAnimatedPause(true);
            }
        }

        private void LoadBackgroundNavBar(UINavigationBar navBar)
        {
            navBar.TintColor = AppStyle.NavigationBarColor;  

            //It might crash on iOS version smaller than 5.0
            try
            {               
                navBar.SetBackgroundImage(UIImage.FromFile("Assets/navBar.png"), UIBarMetrics.Default);
            }
// ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }
    }
}


