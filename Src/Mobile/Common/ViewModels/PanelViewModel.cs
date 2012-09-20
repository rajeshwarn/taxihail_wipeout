using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PanelViewModel : BaseViewModel
	{
		private IAppContext _appContext;

		public PanelViewModel (IAppContext appContext)
		{
			_appContext = appContext;
		}

		public void SignOut()
		{
			_appContext.SignOut ();

			RequestNavigate<LoginViewModel>();
		}
	}
}

