using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.ExtensionMethods;
using Params = System.Collections.Generic.Dictionary<string, string>;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PanelViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>,IMvxServiceConsumer<ICacheService>
    {
        private readonly IAccountService _accountService;
        private readonly BookViewModel _parent;
        public PanelViewModel ( BookViewModel parent )
        {
            _parent = parent;
            _accountService = this.GetService<IAccountService> ();
        }
        public bool TutorialEnabled {
            get{
                return Config.GetSetting("Client.TutorialEnabled", true);
            }
        }

        private bool _menuIsOpen;
        public bool MenuIsOpen {
            get {
                return _menuIsOpen;
            }
            set {
                if (value != _menuIsOpen) {
                    _menuIsOpen = value;
                    FirePropertyChanged ("MenuIsOpen");
                }
            }
        }

        public IMvxCommand SignOut
        {
            get {
                return new MvxRelayCommand(() =>
                {
                    MenuIsOpen = false;
                    _accountService.SignOut ();         
                    RequestNavigate<LoginViewModel> (true);

                    RequestClose( _parent );
                });
            }
        }

        public IMvxCommand NavigateToOrderHistory {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    RequestNavigate<HistoryViewModel> ();
                });
            }
        }

        public IMvxCommand NavigateToMyLocations
        {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    RequestNavigate<MyLocationsViewModel> ();
                });
            }
        }

        private string _version;
        public string Version {
            get {
                return _version;                         
            }
            set {
                if (value != _version) {
                    _version = value;
                    FirePropertyChanged ("Version");
                }
            }
        }

        public IMvxCommand NavigateToUpdateProfile {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    RequestNavigate<RideSettingsViewModel> (new { bookingSettings=  _accountService.CurrentAccount.Settings.ToJson()  });
                });
            }
        }

        public IMvxCommand NavigateToAboutUs {
            get {
                return GetCommand(() => RequestNavigate<AboutUsViewModel>());
            }
        }

        public IMvxCommand NavigateToTutorial {
            get {
                return GetCommand(() =>
                {

                        if ( TutorialEnabled )
                    {
                        MenuIsOpen = false;
                        MessageService.ShowDialogActivity (typeof(TutorialViewModel));
                    }
                });
            }
        }

        
        public bool CanCall
        {
            get
			{
				return !ConfigurationManager.GetSetting("Client.HideCallDispatchButton", false);
			}
        }

        
        public bool CanReportProblem
        {
            get { 
				return !ConfigurationManager.GetSetting("Client.HideReportProblem", false);
			}            
        }

        public IMvxCommand Call {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    Action call = () => { PhoneService.Call (Config.GetSetting( "DefaultPhoneNumber" )); };
                    MessageService.ShowMessage (string.Empty, 
                                                Config.GetSetting( "DefaultPhoneNumberDisplay" ), 
                                               Resources.GetString ("CallButton"), 
                                               call, Resources.GetString ("CancelBoutton"), 
                                               () => {});
                });
            }
        }

        public IMvxCommand ReportProblem {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    InvokeOnMainThread( ()=> PhoneService.SendFeedbackErrorLog (Settings.ErrorLog, Config.GetSetting( "Client.SupportEmail" ) , Resources.GetString ("TechSupportEmailTitle")) );
                });
            }
        }
    }
}

