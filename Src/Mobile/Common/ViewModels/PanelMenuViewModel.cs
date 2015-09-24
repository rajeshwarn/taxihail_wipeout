using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Infrastructure;
using Cirrious.MvvmCross.Plugins.WebBrowser;
using ServiceStack.Text;
using Params = System.Collections.Generic.Dictionary<string, string>;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using System.Threading.Tasks;
using apcurium.MK.Booking.Api.Contract.Resources.Payments;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
	public partial class PanelMenuViewModel : BaseViewModel
    {
	    private readonly IMvxWebBrowserTask _browserTask;

		private readonly IOrderWorkflowService _orderWorkflowService;
		private readonly IAccountService _accountService;
		private readonly IPhoneService _phoneService;
		private readonly IPaymentService _paymentService;
		private readonly IPromotionService _promotionService;

		private bool _isCreatingMenu;

		public PanelMenuViewModel (IMvxWebBrowserTask browserTask, 
			IOrderWorkflowService orderWorkflowService,
			IAccountService accountService,
			IPhoneService phoneService,
			IPaymentService paymentService,
			IPromotionService promotionService)
        {
		    _browserTask = browserTask;
			_orderWorkflowService = orderWorkflowService;
			_accountService = accountService;
			_phoneService = phoneService;
			_paymentService = paymentService;
			_promotionService = promotionService;
			PartialConstructor();
        }

		public async Task Start()
		{
			_isCreatingMenu = true;

			// Initialize list with default values
			InitIOSMenuList();

			// Side panel creation should not block the UI
			await Task.Run (async() => 
				{
					// Load cached payment settings
					var paymentSettings = await _paymentService.GetPaymentSettings();
					IsPayInTaxiEnabled = paymentSettings.IsPayInTaxiEnabled || paymentSettings.PayPalClientSettings.IsEnabled;

					// Load cached settings
					var notificationSettings = await _accountService.GetNotificationSettings(true);

					// Load and cache user notification settings. DO NOT await.
					_accountService.GetNotificationSettings();

					IsNotificationsEnabled = notificationSettings.Enabled;
					IsTaxiHailNetworkEnabled = Settings.Network.Enabled;
					
					
					// Display a watermark indicating on which server the application is pointing
					SetServerWatermarkText();

					// Get the number of active promotions.
					RefreshPromoCodeCountIfNecessary();

					_isCreatingMenu = false;
				});

			// N.B.: This setup is for iOS only! For Android see: SubView_MainMenu.xaml
			InitIOSMenuList();

			if (Settings.PromotionEnabled)
			{
				RefreshIOSMenuBadges();
			}
			else 
			{
				RefreshIOSMenu();
			}
		}
			
		partial void InitIOSMenuList();
		partial void RefreshIOSMenu();
		partial void RefreshIOSMenuBadges();
		partial void PartialConstructor();

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

		private int? _promoCodeAlert;
	    public int? PromoCodeAlert
	    {
	        get
	        {
				return Settings.PromotionEnabled ? _promoCodeAlert : null;
	        }
	        set
	        {
	            _promoCodeAlert = value;
	            RaisePropertyChanged();
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

        public bool DisplayReportProblem
        {
            get 
            { 
                return !Settings.HideReportProblem 
                    && Settings.SupportEmail.HasValue(); 
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

					if (_menuIsOpen && !_isCreatingMenu)
                    {
                        RefreshPromoCodeCountIfNecessary();

						if (Settings.PromotionEnabled)
						{
							RefreshIOSMenuBadges();
						}
                    }

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
				return this.GetCommand(async () =>
                {
						
						await this.Services().Message.ShowMessage(null, 
							this.Services().Localize["PanelMenuViewSignOutPopupMessage"],
                            this.Services().Localize["PanelMenuViewSignOutPopupLogout"],
							()=> 
							{
								CloseMenu();
								_orderWorkflowService.PrepareForNewOrder();
								_accountService.SignOut();         
								ShowViewModelAndClearHistory<LoginViewModel> ();
							},
							this.Services().Localize["Cancel"],
                            ()=> { } 
						);
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
					ShowViewModel<RideSettingsViewModel>();
                });
            }
        }

		public ICommand NavigateToPaymentInformation
		{
			get 
			{
				return this.GetCommand(() =>
				{
                        CloseMenu();

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
            public int ItemMenuId { get; set; }

			public string Text { get; set;}

			public ICommand NavigationCommand{ get; set;}

			public bool Visibility { get; set;}

			public string Alert { get; set; }
		}

		private void CloseMenu()
		{
			IsClosePanelFromMenuItem = true;
			MenuIsOpen = false;
		}

		private async void RefreshPromoCodeCountIfNecessary()
		{
			try
			{
				if (Settings.PromotionEnabled)
				{
					var promoCodes = await _promotionService.GetActivePromotions();

					PromoCodeAlert = promoCodes.Any()
						? (int?)promoCodes.Length
						: null;
				}
			}
			catch (Exception ex)
			{
				Logger.LogError(ex);
			}
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

