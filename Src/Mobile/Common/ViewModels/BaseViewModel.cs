using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ExtensionMethods;
using Cirrious.MvvmCross.Interfaces.Commands;
using Cirrious.MvvmCross.Interfaces.ServiceProvider;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.AppServices.Impl;
using apcurium.MK.Booking.Mobile.Infrastructure;
using apcurium.MK.Common.Diagnostic;
using System.Collections.Generic;
using System;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel : MvxViewModel,
                                 IMvxServiceConsumer<ITinyMessengerHub>,
                                 IMvxServiceConsumer<IAppResource>,
                                 IMvxServiceConsumer<IAppSettings>,
                                 IMvxServiceConsumer<IMessageService>,
                                 IMvxServiceConsumer<ILogger>,
                                 IMvxServiceConsumer<IPhoneService>
    {
        protected BaseViewModel()
        {
            MessengerHub = this.GetService<ITinyMessengerHub>();
            Resources = this.GetService<IAppResource>();
            Settings = this.GetService<IAppSettings>();
            MessageService = this.GetService<IMessageService>();
            Logger = this.GetService<ILogger>();
            PhoneService = this.GetService<IPhoneService>();

            Initialize();
        }

        protected ILogger Logger { get; private set; }

        protected IMessageService MessageService { get; private set; }

        protected IAppResource Resources { get; private set; }

        protected ITinyMessengerHub MessengerHub { get; private set; }

        protected IAppSettings Settings { get; private set; }

        protected IPhoneService PhoneService { get; private set; }

        protected virtual void Initialize()
        {
        }

        public virtual void OnViewLoaded()
        {
            Logger.LogMessage("View loaded: " + GetType().Name);
        }

        public virtual void OnViewUnloaded()
        {
            Logger.LogMessage("View unloaded: " + GetType().Name);
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
    }

}

