using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using ServiceStack.Text;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Requests;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Booking.Mobile.Messages;

namespace apcurium.MK.Booking.Mobile.ViewModels.Callbox
{
    public abstract class BaseCallboxViewModel : BaseViewModel, IMvxServiceConsumer<IBookingService>,
        IMvxServiceConsumer<IAccountService>,
        IMvxServiceConsumer<IAppSettings>,
        IMvxServiceConsumer<ICacheService>
    {
     
		public IMvxCommand Logout
        {
            get
            {
                return this.GetCommand(() => this.MessageService.ShowMessage(this.Resources.GetString("LogoutTitle"), this.Resources.GetString("LogoutMessage"), this.Resources.GetString("Yes"), () =>
					{}, this.Resources.GetString("No"), () => { }));
            }
        }
    }
}