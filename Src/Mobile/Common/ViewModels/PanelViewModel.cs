using System;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Commands;
using Params = System.Collections.Generic.Dictionary<string, string>;
using ServiceStack.Text;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class PanelViewModel : BaseViewModel
    {
        private readonly BookViewModel _parent;
        public PanelViewModel (BookViewModel parent)
        {
            _parent = parent;
        }
        public bool TutorialEnabled {
            get{
                return this.Services().Config.GetSetting("Client.TutorialEnabled", true);
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
                    this.Services().Account.SignOut();         
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
                    RequestNavigate<RideSettingsViewModel>(new { bookingSettings = this.Services().Account.CurrentAccount.Settings.ToJson() });
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
                        this.Services().Message.ShowDialogActivity(typeof(TutorialViewModel));
                    }
                });
            }
        }

        
        public bool CanCall
        {
            get
			{
                return !this.Services().Config.GetSetting("Client.HideCallDispatchButton", false);
			}
        }

        
        public bool CanReportProblem
        {
            get {
                return !this.Services().Config.GetSetting("Client.HideReportProblem", false);
			}            
        }

        public IMvxCommand Call {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    Action call = () => { this.Services().Phone.Call(this.Services().Config.GetSetting("DefaultPhoneNumber")); };
                    this.Services().Message.ShowMessage(string.Empty,
                                                this.Services().Config.GetSetting("DefaultPhoneNumberDisplay"),
                                               this.Services().Resources.GetString("CallButton"),
                                               call, this.Services().Resources.GetString("CancelBoutton"), 
                                               () => {});
                });
            }
        }

        public IMvxCommand ReportProblem {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    InvokeOnMainThread(() => this.Services().Phone.SendFeedbackErrorLog(this.Services().Settings.ErrorLog, this.Services().Config.GetSetting("Client.SupportEmail"), this.Services().Resources.GetString("TechSupportEmailTitle")));
                });
            }
        }
    }
}

