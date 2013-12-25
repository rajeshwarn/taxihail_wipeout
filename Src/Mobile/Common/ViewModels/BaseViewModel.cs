using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using TinyMessenger;
using apcurium.MK.Booking.Api.Client;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using System.Collections.Generic;
using System;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Common.Configuration;
using System.Runtime.CompilerServices;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel : MvxViewModel,
                                 IMvxServiceConsumer<ITinyMessengerHub>,
                                 IMvxServiceConsumer<IAppResource>,
                                 IMvxServiceConsumer<IAppSettings>,
                                 IMvxServiceConsumer<IMessageService>,
                                    IMvxServiceConsumer<ILogger>,
                                    IMvxServiceConsumer<IConfigurationManager>,
                                 IMvxServiceConsumer<IPhoneService>
    {
        protected BaseViewModel()
        {
            MessengerHub = this.GetService<ITinyMessengerHub>();
            Resources = this.GetService<IAppResource>();
            Settings = this.GetService<IAppSettings>();
            MessageService = this.GetService<IMessageService>();
            Logger = this.GetService<ILogger>();
            Config = this.GetService<IConfigurationManager>();
            PhoneService = this.GetService<IPhoneService>();


            Initialize();
        }
        
        public static Action NoAction = () => { };

        protected ILogger Logger { get; private set; }

        protected IConfigurationManager Config { get; private set; }

        protected IMessageService MessageService { get; private set; }

        protected IAppResource Resources { get; private set; }

        protected ITinyMessengerHub MessengerHub { get; private set; }

        public IAppSettings Settings { get; private set; }

        protected IPhoneService PhoneService { get; private set; }

        private AbstractLocationService _locationService;
        protected AbstractLocationService LocationService{
            get {
                return _locationService ?? (_locationService = TinyIoCContainer.Current.Resolve<AbstractLocationService>());
            }
        }

        private IBookingService _bookingService;
        protected IBookingService BookingService{
            get { return _bookingService ?? (_bookingService = TinyIoCContainer.Current.Resolve<IBookingService>()); }
        }
        
        protected ICacheService CacheService{
            get{
                return TinyIoCContainer.Current.Resolve<ICacheService> ();
            }
        }

        protected IAppCacheService AppCacheService{
            get{
                return TinyIoCContainer.Current.Resolve<IAppCacheService> ();
            }
        }

		protected IAppSettings AppSettings{
			get{
				return TinyIoCContainer.Current.Resolve<IAppSettings> ();
			}
		}   



        protected IApplicationInfoService ApplicationInfoService{
            get{
                return TinyIoCContainer.Current.Resolve<IApplicationInfoService> ();
            }
        }

            
        protected IGeolocService GeolocService{
            get{
                return TinyIoCContainer.Current.Resolve<IGeolocService> ();
            }
        }

        protected IAccountService AccountService{
            get{
                return TinyIoCContainer.Current.Resolve<IAccountService> ();
            }
        }
        
		private IPaymentService _paymentService;
		protected IPaymentService PaymentService{
            get{
				if(_paymentService==null)
				{
					_paymentService = TinyIoCContainer.Current.Resolve<IPaymentService> ();
				}

				return _paymentService;
            }
			set{
				_paymentService = value;
			}
        }
        
        protected IVehicleClient VehicleClient{
            get{
                return TinyIoCContainer.Current.Resolve<IVehicleClient> ();
            }
        }
        
        protected IConfigurationManager ConfigurationManager{
            get{
                return TinyIoCContainer.Current.Resolve<IConfigurationManager> ();
            }
        }
        protected virtual void Initialize()
        {
        }

        public virtual void Load()
        {

        }

        public virtual void Start (bool firstStart = false)
        {
            firstStart.ToString();
        }

        public virtual void Restart ()
        {
            
        }

        public virtual void Stop ()
        {

        }

        public virtual void Unload ()
        {

        }


        protected bool RequestSubNavigate<TViewModel, TResult>(IDictionary<string, string> parameterValues,
                                                               Action<TResult> onResult)
            where TViewModel : BaseSubViewModel<TResult>
        {
            parameterValues = parameterValues ?? new Dictionary<string, string>();

            if (parameterValues.ContainsKey("messageId"))
                throw new ArgumentException("parameterValues cannot contain an item with the key 'messageId'");

            string messageId = Guid.NewGuid().ToString();

            parameterValues["messageId"] = messageId;

            TinyMessageSubscriptionToken token = null;
            token = MessengerHub.Subscribe<SubNavigationResultMessage<TResult>>(msg =>
                                                                                    {
                                                                                        if (token != null)
                                                                                            MessengerHub
                                                                                                .Unsubscribe
                                                                                                <
                                                                                                    SubNavigationResultMessage
                                                                                                        <TResult>>(token);

                                                                                        onResult(msg.Result);
                                                                                    },
                                                                                msg => msg.MessageId == messageId);

            return RequestNavigate<TViewModel>(parameterValues);
        }

        protected IMvxCommand GetCommand(Action action)
        {
            return new AsyncCommand(action);
        }

        protected IMvxCommand GetCommand<T>(Action<T> action)
        {
            return new AsyncCommand<T>(action);
        }

		readonly IDictionary<string, IMvxCommand> _commands = new Dictionary<string, IMvxCommand>();
		protected IMvxCommand GetCommand(Action execute, Func<bool> canExecute, [CallerMemberName] string memberName = null)
		{
			return _commands.ContainsKey(memberName)
				? _commands[memberName]
					: (_commands[memberName] = new MvxRelayCommand(execute, canExecute));

		}
    }

}

