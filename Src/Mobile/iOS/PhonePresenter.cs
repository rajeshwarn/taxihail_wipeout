using System;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Touch.Interfaces;
using Cirrious.MvvmCross.Platform.Diagnostics;
using Cirrious.MvvmCross.Interfaces.Platform.Diagnostics;
using Cirrious.MvvmCross.ExtensionMethods;
using apcurium.MK.Booking.Mobile.Client.Navigation;
using System.Linq;
using Cirrious.MvvmCross.Views;

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
            navBar.Frame = new System.Drawing.RectangleF(0, 20, 320, 44);
            _masterNavigationController.View.InsertSubview(navBar, 0);

            var back = new UIImageView();
            back.Image = UIImage.FromFile("Assets/background.png");
            back.Frame = new System.Drawing.RectangleF(0, 64, 320, back.Image.Size.Height);
            _masterNavigationController.View.InsertSubview(back, 0);

        }

        private IMvxTouchView _last;

        public override void Show(Cirrious.MvvmCross.Views.MvxShowViewModelRequest request)
        {
            var view = CreateView(request);

            if ((_last != null) && (_last.GetType() == view.GetType()))
            {
                view = _last;
            }

                
            _last = view;               

            if (request.ClearTop)
                ClearBackStack();

            Show(view);


        }

        private IMvxTouchView CreateView(MvxShowViewModelRequest request)
        {
            return this.GetService().CreateView(request);
        }

        public override void Show(Cirrious.MvvmCross.Touch.Interfaces.IMvxTouchView view)
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

        private bool HideNavBar(Cirrious.MvvmCross.Touch.Interfaces.IMvxTouchView view)
        {
            return (view is INavigationView) && (((INavigationView)view).HideNavigationBar);
        }

        private UIViewController _modal;

        public override bool PresentModalViewController(UIViewController viewController, bool animated)
        {

            if (CurrentTopViewController != null)
            {

                viewController.View.Frame = new System.Drawing.RectangleF(0, 0, CurrentTopViewController.View.Bounds.Width, CurrentTopViewController.View.Bounds.Height);
                _modal = viewController;
                CurrentTopViewController.View.AddSubview(viewController.View);
                return true;
            }
            return false;

            //CurrentTopViewController.NavigationController.ModalPresentationStyle = UIModalPresentationStyle.CurrentContext; 
            //return base.PresentModalViewController (viewController, animated);
        }

        public override void Close(Cirrious.MvvmCross.Interfaces.ViewModels.IMvxViewModel toClose)
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
            catch
            {
            }
        }
    }
}


