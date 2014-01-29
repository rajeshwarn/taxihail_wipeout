using System;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using System.Collections.Generic;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.Data;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class HomeViewModel : BaseViewModel
    {
		public HomeViewModel(IMvxWebBrowserTask browserTask) : base()
		{
			Panel = new PanelMenuViewModel(this, browserTask);
		}

		public void Init()
		{
		}

		public PanelMenuViewModel Panel { get; set; }
    }
}

