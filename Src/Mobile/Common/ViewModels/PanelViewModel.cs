using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using Params = System.Collections.Generic.Dictionary<string, string>;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PanelViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>
	{
        readonly IAccountService _accountService;
		public PanelViewModel ()
		{
            _accountService = this.GetService<IAccountService>();
		}

		public MvxRelayCommand SignOut
		{
			get
			{
				return new MvxRelayCommand(() =>
			{
					_accountService.SignOut();			
					InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new LogOutRequested(this)));
				});
			}
		}


		public MvxRelayCommand NavigateToOrderHistory
		{
			get
			{
				return new MvxRelayCommand(() =>
				                           {
					RequestNavigate<HistoryViewModel>();
				});
			}
		}

		private string _version;
		public string Version {
			get
			{
				return _version;				         
			}
			set 
			{
				if(value != _version)
				{
					_version = value;
					FirePropertyChanged("Version");
				}
			}
		}

        public IMvxCommand NavigateToUpdateProfile
        {
            get
            {
                return new MvxRelayCommand(()=>{
                    RequestSubNavigate<RideSettingsViewModel, BookingSettings>(new Params{
                        { "bookingSettings", _accountService.CurrentAccount.Settings.ToJson()  }
                    }, result => {
                        if(result!=null)
                        {
                            _accountService.UpdateSettings(result);
                        }
                    });
                });
            }
        }

        public IMvxCommand Call
        {
            get
            {
                return new MvxRelayCommand(()=>{
                    Action call = () => { PhoneService.Call(Settings.PhoneNumber(_accountService.CurrentAccount.Settings.ProviderId.Value)); };
                    MessageService.ShowMessage(string.Empty, 
                                               Settings.PhoneNumberDisplay(_accountService.CurrentAccount.Settings.ProviderId.Value), 
                                               Resources.GetString("CallButton"), 
                                               call, Resources.GetString("CancelBoutton"), 
                                               () => {});                    
                });
            }
        }

        public IMvxCommand ReportProblem
        {
            get
            {
                return new MvxRelayCommand(()=>{
                           
                });
            }
        }


	}
}

