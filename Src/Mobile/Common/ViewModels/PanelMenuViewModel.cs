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
		private readonly BaseViewModel _parent;
		private IMvxWebBrowserTask _browserTask;

		public PanelMenuViewModel (BaseViewModel parent, IMvxWebBrowserTask browserTask)
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
            get
            {
				return Settings.TutorialEnabled;
            }
        }

        private bool _menuIsOpen;
        public bool MenuIsOpen {
            get 
            {
                return _menuIsOpen;
            }
            set 
            {
                if (value != _menuIsOpen) 
                {
                    _menuIsOpen = value;
					RaisePropertyChanged ();
                }
            }
        }

		public ICommand OpenOrCloseMenu
		{
			get 
			{
				return this.GetCommand(() =>
					{
						MenuIsOpen = !MenuIsOpen;
					});
			}
		}

		public ICommand  ToApcuriumWebsite
		{
			get 
			{
				return this.GetCommand(() =>
				{
					_browserTask.ShowWebPage(this.Services().Localize["apcuriumUrl"]);
				});
			}
		}

		public ICommand  ToMobileKnowledgeWebsite
		{
			get 
			{
				return this.GetCommand(() =>
				{
					_browserTask.ShowWebPage(this.Services().Localize["mobileKnowledgeUrl"]);
				});
			}
		}

		public ICommand SignOut
        {
            get 
			{
				return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
                    this.Services().Account.SignOut();         
                    ShowViewModel<LoginViewModel> (true);

                    Close( _parent );
                });
            }
        }

		public ICommand NavigateToOrderHistory
        {
            get 
            {
                return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
						ShowViewModel<HistoryListViewModel> ();
                });
            }
        }

		public ICommand NavigateToMyLocations
        {
            get 
            {
                return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
						ShowViewModel<LocationListViewModel> ();
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
                if (value != _version) 
                {
                    _version = value;
					RaisePropertyChanged ();
                }
            }
        }

		public ICommand NavigateToUpdateProfile
        {
            get 
            {
                return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
                    ShowViewModel<RideSettingsViewModel>(new { bookingSettings = this.Services().Account.CurrentAccount.Settings.ToJson() });
                });
            }
        }

		public ICommand NavigateToAboutUs
        {
            get 
            {
                return this.GetCommand(() => 
                {
                    MenuIsOpen = false;
                    ShowViewModel<AboutUsViewModel>();
                });
            }
        }

		public ICommand NavigateToReportProblem
		{
			get 
            {
				return this.GetCommand(() =>
				{
					MenuIsOpen = false;
					InvokeOnMainThread(() => this.Services().Phone.SendFeedbackErrorLog(Settings.SupportEmail, this.Services().Localize["TechSupportEmailTitle"]));
				});
			}
		}

		public ICommand NavigateToTutorial
        {
            get 
            {
                return this.GetCommand(() =>
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
				return !Settings.HideCallDispatchButton;
			}
        }

        public bool CanReportProblem
        {
            get 
            {
				return !Settings.HideReportProblem;
			}            
        }

		public ICommand Call
        {
            get 
            {
                return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
						Action call = () => { this.Services().Phone.Call(Settings.DefaultPhoneNumber); };
                    this.Services().Message.ShowMessage(string.Empty,
												Settings.DefaultPhoneNumberDisplay,
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

