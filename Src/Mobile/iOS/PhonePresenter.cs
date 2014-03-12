using System.Collections.Generic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Touch.Views.Presenters;
using Cirrious.MvvmCross.ViewModels;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.PresentationHints;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class PhonePresenter: MvxModalSupportTouchViewPresenter
    {
		public PhonePresenter(UIApplicationDelegate applicationDelegate, UIWindow window)
            : base(applicationDelegate, window)
        {
        }

        protected override UINavigationController CreateNavigationController(UIViewController viewController)
        {
            var navController = new UINavigationController(viewController);
            Mvx.RegisterSingleton<UINavigationController>(navController);
            return navController;
        }

        public override void Show(MvxViewModelRequest request)
        {
            base.Show(request);
            if(request.ParameterValues != null
                && request.ParameterValues.ContainsKey("removeFromHistory"))
            {
                RemovePreviousViewFromHistory();
            }
        }

        public override void ChangePresentation(MvxPresentationHint hint)
        {
            if (hint is ChangePresentationHint)
            {
                TryChangeViewPresentation((ChangePresentationHint)hint);
            }
            else
            {
                base.ChangePresentation(hint);
            }
        }

        private void RemovePreviousViewFromHistory()
        { 
            var navController = Mvx.Resolve<UINavigationController>();

            var controllers = navController.ViewControllers;
            if (controllers.Length > 1)
            {
                var listOfControllers = new List<UIViewController>(controllers);
                listOfControllers.RemoveAt(listOfControllers.Count - 2);
                navController.ViewControllers = listOfControllers.ToArray();
            }
            else
            {
                Mvx.Warning("Can't remove previous view, not enough UIViewControllers in the stack");
            }

        }

        private void TryChangeViewPresentation(ChangePresentationHint hint)
        {
            var view = CurrentTopViewController as IChangePresentation;
            if (view != null)
            {
                view.ChangePresentation(hint);
                foreach (var subview in CurrentTopViewController.View.FindSubviewsOfType<IChangePresentation>())
                {
                    subview.ChangePresentation(hint);
                }
            }
            else
            {
                Mvx.Warning("Can't change presentation, view controller doesn't support IChangePresentation");
            }

        }
    }
}


