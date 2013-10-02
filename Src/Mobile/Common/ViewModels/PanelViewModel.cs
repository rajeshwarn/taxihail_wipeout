using System;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
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
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Threading.Tasks;
using System.Threading;
using apcurium.MK.Common.Configuration;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PanelViewModel : BaseViewModel, IMvxServiceConsumer<IAccountService>,IMvxServiceConsumer<ICacheService>
    {
        readonly IAccountService _accountService;
        private BookViewModel _parent;
        public PanelViewModel ( BookViewModel parent )
        {
            _parent = parent;
            _accountService = this.GetService<IAccountService> ();

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            var canCall = Task.Run(() => !ConfigurationManager.GetSetting("Client.HideCallDispatchButton").TryToParse(true));
            var canReportProblem = Task.Run(() => !ConfigurationManager.GetSetting("Client.HideReportProblem").TryToParse(true));

            this.CanCall = await canCall;
            this.CanReportProblem = await canReportProblem;
        }

        private bool _menuIsOpen = false;

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
                return GetCommand(() =>
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
                    if ( Settings.TutorialEnabled )
                    {
                        MenuIsOpen = false;
                        MessageService.ShowDialogActivity (typeof(TutorialViewModel));
                    }
                });
            }
        }

        public bool CanCall
        {
            get;
            private set;
        }

        public bool CanReportProblem
        {
            get;
            private set;
        }

        public IMvxCommand Call {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    Action call = () => { PhoneService.Call (Settings.PhoneNumber (_accountService.CurrentAccount.Settings.ProviderId)); };
                    MessageService.ShowMessage (string.Empty, 
                                               Settings.PhoneNumberDisplay (_accountService.CurrentAccount.Settings.ProviderId), 
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
                    InvokeOnMainThread( ()=> PhoneService.SendFeedbackErrorLog (Settings.ErrorLog, Settings.SupportEmail, Resources.GetString ("TechSupportEmailTitle")) );
                });
            }
        }
    }
}

