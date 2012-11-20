using System;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Common.Entity;
using TinyIoC;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using ServiceStack.Text;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;

namespace apcurium.MK.Booking.Mobile
{
	public class LocationDetailViewModel: BaseViewModel
	{
		public LocationDetailViewModel ()
		{
		}

		private Address _address;
		public Address Address 
		{
			get
			{
				return _address;
			}

			set 
			{
				if(value != _address)
				{
					_address = value;
					FirePropertyChanged("Address");
				}
			}
		}

		public IMvxCommand RebookOrder
		{
			get { return new MvxRelayCommand(()=>
				                                 {
                 var order = new Order();
                 order.PickupAddress = Address;
                 var account = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
                 order.Settings = account.Settings;
                 var serialized = JsonSerializer.SerializeToString(order);
				 RequestNavigate<BookViewModel>(new { order = serialized }, clearTop: true, requestedBy: MvxRequestedBy.UserAction);
				});
			}
		}
	}
}

