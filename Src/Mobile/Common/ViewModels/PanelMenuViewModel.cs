using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Configuration;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using ServiceStack.Text;
using Params = System.Collections.Generic.Dictionary<string, string>;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using apcurium.MK.Common.Configuration.Impl;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PanelMenuViewModel : BaseViewModel
    {
		private readonly BaseViewModel _parent;
		private readonly IMvxWebBrowserTask _browserTask;

		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
		private readonly IPhoneService _phoneService;
		private readonly IPaymentService _paymentService;

		public PanelMenuViewModel (BaseViewModel parent, 
			IMvxWebBrowserTask browserTask, 
			IOrderWorkflowService orderWorkflowService,
			IAccountService accountService,
			IPhoneService phoneService,
			IPaymentService paymentService)
        {
            _parent = parent;
			_browserTask = browserTask;

			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
			_phoneService = phoneService;
			_paymentService = paymentService;
			ItemMenuList = new ObservableCollection<ItemMenuModel>();
        }

		public async Task Start()
		{
			var paymentSettings = _paymentService.GetPaymentSettings();
			IsPayInTaxiEnabled = paymentSettings.IsPayInTaxiEnabled;

		    var notificationSettings = await _accountService.GetNotificationSettings(true);
		    IsNotificationsEnabled = notificationSettings.Enabled;

			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewLocationsText"], NavigationCommand = NavigateToMyLocations});
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewOrderHistoryText"], NavigationCommand = NavigateToOrderHistory});
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewUpdateProfileText"], NavigationCommand = NavigateToUpdateProfile});
			if (IsPayInTaxiEnabled)
				ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewPaymentInfoText"], NavigationCommand = NavigateToPaymentInformation});
            if (IsNotificationsEnabled)
                ItemMenuList.Add(new ItemMenuModel() {Text = this.Services().Localize["PanelMenuViewNotificationsText"], NavigationCommand = NavigateToNotificationsSettings});
			if (Settings.TutorialEnabled)
				ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewTutorialText"], NavigationCommand = NavigateToTutorial});
			if (!Settings.HideCallDispatchButton)
				ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewCallDispatchText"], NavigationCommand = Call});
			ItemMenuList.Add(new ItemMenuModel(){Text = this.Services().Localize["PanelMenuViewAboutUsText"], NavigationCommand = NavigateToAboutUs});
			if (!Settings.HideReportProblem)
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

	    private bool _isPayInTaxiEnabled;
        public bool IsPayInTaxiEnabled
	    {
	        get
	        {
                return _isPayInTaxiEnabled;
	        }
	        set
	        {
                if (_isPayInTaxiEnabled != value)
	            {
                    _isPayInTaxiEnabled = value;
	                RaisePropertyChanged();
	            }
	        }
	    }

        private bool _isNotificationsEnabled;
        public bool IsNotificationsEnabled
        {
            get
            {
                return _isNotificationsEnabled;
            }
            set
            {
                if (_isNotificationsEnabled != value)
                {
                    _isNotificationsEnabled = value;
                    RaisePropertyChanged();
                }
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
					_orderWorkflowService.PrepareForNewOrder();
					_accountService.SignOut();         
					ShowViewModel<LoginViewModel> ();

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
				if (string.IsNullOrEmpty(_version))
				{
					_version = this.Services().PackageInfo.Version;
				}
                return _version;                         
            }
        }

		public ICommand NavigateToUpdateProfile
        {
            get 
            {
                return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
					ShowViewModel<RideSettingsViewModel>(new { bookingSettings = _accountService.CurrentAccount.Settings.ToJson() });
                });
            }
        }

		public ICommand NavigateToPaymentInformation
		{
			get 
			{
				return this.GetCommand(() =>
				{
					MenuIsOpen = false;
					ShowViewModel<CreditCardAddViewModel>();
				});
			}
		}

        public ICommand NavigateToNotificationsSettings
        {
            get
            {
                return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
                    ShowViewModel<NotificationSettingsViewModel>();
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
					InvokeOnMainThread(() => _phoneService.SendFeedbackErrorLog(Settings.SupportEmail, this.Services().Localize["TechSupportEmailTitle"]));
				});
			}
		}

		public ICommand NavigateToTutorial
        {
            get 
            {
                return this.GetCommand(() =>
                {
					if (Settings.TutorialEnabled)
                    {
                        MenuIsOpen = false;
                        this.Services().Message.ShowDialog(typeof(TutorialViewModel));
                    }
                });
            }
        }

		public ICommand Call
        {
            get 
            {
                return this.GetCommand(() =>
                {
                    MenuIsOpen = false;
					Action call = () => { _phoneService.Call(Settings.DefaultPhoneNumber); };
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

