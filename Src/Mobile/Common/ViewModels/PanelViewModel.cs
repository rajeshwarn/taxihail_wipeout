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

        private bool _menuIsOpen = false;
        public bool MenuIsOpen {
            get {
                return _menuIsOpen;
            }
            set{
                if(value!= _menuIsOpen)
                {
                    _menuIsOpen = value;
                    FirePropertyChanged("MenuIsOpen");
                }
            }
        }

		public MvxRelayCommand SignOut
		{
			get
			{
				return new MvxRelayCommand(() =>
			{
                    MenuIsOpen = false;
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
                    MenuIsOpen = false;
					RequestNavigate<HistoryViewModel>();
				});
			}
		}

        public MvxRelayCommand NavigateToMyLocations
        {
            get
            {
                return new MvxRelayCommand(() =>
                                           {
                    MenuIsOpen = false;
                    RequestNavigate<MyLocationsViewModel>();
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
                    MenuIsOpen = false;
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
                    MenuIsOpen = false;
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
                    MenuIsOpen = false;
                    PhoneService.SendFeedbackErrorLog(Settings.ErrorLog, Settings.SupportEmail, Resources.GetString("TechSupportEmailTitle"));        
                });
            }
        }


	}
}

