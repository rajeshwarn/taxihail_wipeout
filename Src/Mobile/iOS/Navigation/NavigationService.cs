//using System;
//using apcurium.MK.Booking.Mobile.Navigation;
//using apcurium.MK.Booking.Mobile.ViewModels;
//using MonoTouch.UIKit;
//
//namespace apcurium.MK.Booking.Mobile.Client
//{
//    public class NavigationService : INavigationService
//    {
//        RootTabController _rootController;
//        public NavigationService(RootTabController rootController)
//        {
//            _rootController = rootController;
//        }
//
//        #region INavigationService implementation
//
//        public void Navigate<TViewModel,TView>() where TViewModel : BaseViewModel
//        {
//            var viewModel = TinyIoC.TinyIoCContainer.Current.Resolve<TViewModel>();
//            viewModel.Load();
//            var view = Activator.CreateInstance<TView>() as UIViewController;
//            _rootController.Navigate( view );
//        }
//
//        public void Navigate<TViewModel>(string viewTypeName) where TViewModel : BaseViewModel
//        {
//            var viewType = Type.GetType( viewTypeName );
//            var viewModel = TinyIoC.TinyIoCContainer.Current.Resolve<TViewModel>();
//            viewModel.Load();
//            var view = Activator.CreateInstance(viewType) as UIViewController;
//            _rootController.Navigate( view );
//        }
//
//
//        #endregion
//    }
//}

