using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using ServiceStack.Text;
using Params = System.Collections.Generic.Dictionary<string, string>;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public class PanelMenuViewModel : BaseViewModel
    {
	    private readonly IMvxWebBrowserTask _browserTask;

		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
		private readonly IPhoneService _phoneService;
		private readonly IPaymentService _paymentService;

		public PanelMenuViewModel (IMvxWebBrowserTask browserTask, 
			IOrderWorkflowService orderWorkflowService,
			IAccountService accountService,
			IPhoneService phoneService,
			IPaymentService paymentService)
        {
		    _browserTask = browserTask;

			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
			_phoneService = phoneService;
			_paymentService = paymentService;
			ItemMenuList = new ObservableCollection<ItemMenuModel>();
        }

		public async Task Start()
		{
            // N.B.: This setup is for iOS only! For Android see: SubView_MainMenu.xaml

			// Load cached payment settings
			var paymentSettings = await _paymentService.GetPaymentSettings();
			IsPayInTaxiEnabled = paymentSettings.IsPayInTaxiEnabled || paymentSettings.PayPalClientSettings.IsEnabled;

			// Load cached settings
		    var notificationSettings = await _accountService.GetNotificationSettings(true);

            // Load and cache user notification settings. DO NOT await.
#pragma warning disable 4014
            _accountService.GetNotificationSettings();
#pragma warning restore 4014

		    IsNotificationsEnabled = notificationSettings.Enabled;
            IsTaxiHailNetworkEnabled = Settings.Network.Enabled;

            // Display a watermark indicating on which server the application is pointing
            SetServerWatermarkText();

			ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewLocationsText"], NavigationCommand = NavigateToMyLocations });
			ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewOrderHistoryText"], NavigationCommand = NavigateToOrderHistory });
			ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewUpdateProfileText"], NavigationCommand = NavigateToUpdateProfile });
		    if (IsPayInTaxiEnabled)
		    {
                ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewPaymentInfoText"], NavigationCommand = NavigateToPaymentInformation });
		    }
		    if (Settings.PromotionEnabled)
		    {
		        ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewPromotionsText"], NavigationCommand = NavigateToPromotions });
		    }
		    if (IsNotificationsEnabled)
		    {
                ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewNotificationsText"], NavigationCommand = NavigateToNotificationsSettings });
		    }			
            if (IsTaxiHailNetworkEnabled)
            {
                ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewTaxiHailNetworkText"], NavigationCommand = NavigateToUserTaxiHailNetworkSettings });
            }
		    if (Settings.TutorialEnabled)
		    {
                ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewTutorialText"], NavigationCommand = NavigateToTutorial });
		    }
		    if (!Settings.HideCallDispatchButton)
		    {
                ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewCallDispatchText"], NavigationCommand = Call });
		    }
			ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewAboutUsText"], NavigationCommand = NavigateToAboutUs });
		    if (!Settings.HideReportProblem)
		    {
                ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewReportProblemText"], NavigationCommand = NavigateToReportProblem });
		    }
			ItemMenuList.Add(new ItemMenuModel { Text = this.Services().Localize["PanelMenuViewSignOutText"], NavigationCommand = SignOut });
		}

	    public ObservableCollection<ItemMenuModel> ItemMenuList { get; set; }

	    private string _serverWatermarkText;
	    public string ServerWatermarkText
	    {
	        get { return _serverWatermarkText; }
	        set
	        {
	            if (_serverWatermarkText != value)
	            {
	                _serverWatermarkText = value;
                    RaisePropertyChanged();
	            }
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

        private bool _isTaxiHailNetworkEnabled;
        public bool IsTaxiHailNetworkEnabled
        {
            get
            {
                return _isTaxiHailNetworkEnabled;
            }
            set
            {
                if (_isTaxiHailNetworkEnabled != value)
                {
                    _isTaxiHailNetworkEnabled = value;
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

        public bool IsClosePanelFromMenuItem { get; set; }

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
					CloseMenu();
					_orderWorkflowService.PrepareForNewOrder();
					_accountService.SignOut();         
					ShowViewModelAndClearHistory<LoginViewModel> ();
                });
            }
        }

		public ICommand NavigateToOrderHistory
        {
            get 
            {
                return this.GetCommand(() =>
                {
					CloseMenu();
					ShowViewModel<HistoryListViewModel>();
                });
            }
        }

		public ICommand NavigateToMyLocations
        {
            get 
            {
                return this.GetCommand(() =>
                {
					CloseMenu();
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
					CloseMenu();
					ShowViewModel<RideSettingsViewModel>(new { bookingSettings = _accountService.CurrentAccount.Settings.ToJson() });
                });
            }
        }

		public ICommand NavigateToPaymentInformation
		{
			get 
			{
				return this.GetCommand(async () =>
				{
					CloseMenu();
					var overduePayment = await _accountService.GetOverduePayment();
					
					if(overduePayment == null)
					{
						ShowViewModel<CreditCardAddViewModel>();
					}
					else
					{
						ShowViewModel<OverduePaymentViewModel>(new 
						{ 
							overduePayment = overduePayment.ToJson() 
						});
					}
				});
			}
		}

        public ICommand NavigateToNotificationsSettings
        {
            get
            {
                return this.GetCommand(() =>
                {
					CloseMenu();
                    ShowViewModel<NotificationSettingsViewModel>();
                });
            }
        }

        public ICommand NavigateToUserTaxiHailNetworkSettings
        {
            get
            {
                return this.GetCommand(() =>
                {
                    CloseMenu();
                    ShowViewModel<UserTaxiHailNetworkSettingsViewModel>();
                });
            }
        }

		public ICommand NavigateToPromotions
		{
			get 
			{
				return this.GetCommand(() =>
				{
					CloseMenu();
					ShowViewModel<PromotionViewModel> ();
				});
			}
		}

		public ICommand NavigateToAboutUs
        {
            get 
            {
                return this.GetCommand(() => 
                {
					CloseMenu();
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
					CloseMenu();
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
						CloseMenu();
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
					CloseMenu();
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

		private void CloseMenu()
		{
			IsClosePanelFromMenuItem = true;
			MenuIsOpen = false;
		}

	    private void SetServerWatermarkText()
	    {
	        var serverTarget = Settings.ServiceUrl.ToLower();

	        if (serverTarget.Contains("test.taxihail.biz"))
	        {
	            ServerWatermarkText = "Dev Version";
	        }
            else if (serverTarget.Contains("staging"))
            {
                ServerWatermarkText = "Staging Version";
            }
            else if (serverTarget.Contains("localhost") || serverTarget.Contains("apcurium.mk.web"))
            {
                ServerWatermarkText = "Local Version";
            }
            else
            {
                // No watermark for production
                ServerWatermarkText = null;
            }
	    }
    }
}

