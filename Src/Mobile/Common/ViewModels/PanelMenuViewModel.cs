using System;
using apcurium.MK.Booking.Mobile.Extensions;
using Params = System.Collections.Generic.Dictionary<string, string>;
using ServiceStack.Text;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Cirrious.MvvmCross.Plugins.WebBrowser;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PanelMenuViewModel : BaseViewModel
    {
        private readonly BookViewModel _parent;
		private IMvxWebBrowserTask _browserTask;

		public PanelMenuViewModel (BookViewModel parent, IMvxWebBrowserTask browserTask)
        {
            _parent = parent;
			_browserTask = browserTask;

			ItemMenuList = new ObservableCollection<ItemMenuModel>();
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewLocationsText"], NavigationCommand = NavigateToMyLocations});
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewOrderHistoryText"], NavigationCommand = NavigateToOrderHistory});
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewUpdateProfileText"], NavigationCommand = NavigateToUpdateProfile});
			if (TutorialEnabled)
				ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewTutorialText"], NavigationCommand = NavigateToTutorial});
			if (CanCall)
				ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewCallDispatchText"], NavigationCommand = Call});
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewAboutUsText"], NavigationCommand = NavigateToAboutUs});
			if (CanReportProblem)
				ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewReportProblemText"], NavigationCommand = NavigateToReportProblem});
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewSignOutText"], NavigationCommand = SignOut});
        }
		
		private ObservableCollection<ItemMenuModel> _itemMenuList;
		public ObservableCollection<ItemMenuModel> ItemMenuList
		{
			get
			{
				return _itemMenuList;
			}

			set
			{
				_itemMenuList = value;
			}
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
					RaisePropertyChanged ();
                }
            }
        }

		public AsyncCommand  ToApcuriumWebsite
		{
			get {
				return new AsyncCommand(() =>
					{
						_browserTask.ShowWebPage(this.Services().Localize["apcuriumUrl"]);
					});
			}
		}

		public AsyncCommand  ToMobileKnowledgeWebsite
		{
			get {
				return new AsyncCommand(() =>
					{
						_browserTask.ShowWebPage(this.Services().Localize["mobileKnowledgeUrl"]);
					});
			}
		}

        public AsyncCommand SignOut
        {
            get {
                return new AsyncCommand(() =>
                {
                    MenuIsOpen = false;
                    this.Services().Account.SignOut();         
                    ShowViewModel<LoginViewModel> (true);

                    Close( _parent );
                });
            }
        }

        public AsyncCommand NavigateToOrderHistory
        {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    ShowViewModel<HistoryViewModel> ();
                });
            }
        }

        public AsyncCommand NavigateToMyLocations
        {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    ShowViewModel<MyLocationsViewModel> ();
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
					RaisePropertyChanged ();
                }
            }
        }

        public AsyncCommand NavigateToUpdateProfile
        {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    ShowViewModel<RideSettingsViewModel>(new { bookingSettings = this.Services().Account.CurrentAccount.Settings.ToJson() });
                });
            }
        }

        public AsyncCommand NavigateToAboutUs
        {
            get {
                return GetCommand(() => ShowViewModel<AboutUsViewModel>());
            }
        }

		public AsyncCommand NavigateToReportProblem
		{
			get {
				return GetCommand(() =>
					{
						MenuIsOpen = false;
						//InvokeOnMainThread(() => this.Services().Phone.SendFeedbackErrorLog(Settings.ErrorLog, this.Services().Config.GetSetting("Client.SupportEmail"), this.Services().Localize["TechSupportEmailTitle"]));
					});
			}
		}

        public AsyncCommand NavigateToTutorial
        {
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

        public AsyncCommand Call
        {
            get {
                return GetCommand(() =>
                {
                    MenuIsOpen = false;
                    Action call = () => { this.Services().Phone.Call(this.Services().Config.GetSetting("DefaultPhoneNumber")); };
                    this.Services().Message.ShowMessage(string.Empty,
                                                this.Services().Config.GetSetting("DefaultPhoneNumberDisplay"),
                                               this.Services().Localize["CallButton"],
                                               call, this.Services().Localize["Cancel"], 
                                               () => {});
                });
            }
        }

		public class ItemMenuModel
		{
			public string Text { get; set;}

			public ICommand NavigationCommand{ get; set;}

			public bool Visibility { get; set;}
		}
    }
}

