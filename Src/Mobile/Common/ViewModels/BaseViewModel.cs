using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.ViewModels;
using TinyMessenger;
using apcurium.MK.Common.Diagnostic;
using System.Collections.Generic;
using System;
using TinyIoC;
using System.Runtime.CompilerServices;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel : MvxViewModel
    {
        protected BaseViewModel()
        {
        }

        public static Action NoAction = () => { };

        public TinyIoCContainer Container
        {
            get { return TinyIoCContainer.Current; }
        }

        protected ILogger Logger { get { return Container.Resolve<ILogger>(); } }

        public virtual void Load()
        {
        }

        public virtual void Start(bool firstStart = false)
        {
        }

        public virtual void Restart()
        {
        }

        public virtual void Stop()
        {
        }

        public virtual void Unload()
        {
        }

        protected void RequestNavigate<T>(object param, bool clearTop) where T : IMvxViewModel
        {
            RequestNavigate<T>(param, clearTop, MvxRequestedBy.UserAction);
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
            // ReSharper disable once RedundantAssignment
            token = this.Services().MessengerHub.Subscribe<SubNavigationResultMessage<TResult>>(msg =>
            {
                // ReSharper disable AccessToModifiedClosure
                if (token != null)
                    this.Services().MessengerHub.Unsubscribe
                        <SubNavigationResultMessage<TResult>>(token);
                // ReSharper restore AccessToModifiedClosure

                onResult(msg.Result);
            },
            msg => msg.MessageId == messageId);

            return RequestNavigate<TViewModel>(parameterValues);
        }

        protected AsyncCommand GetCommand(Action action)
        {
            return new AsyncCommand(action);
        }

        protected AsyncCommand<T> GetCommand<T>(Action<T> action)
        {
            return new AsyncCommand<T>(action);
        }

        readonly IDictionary<string, AsyncCommand> _commands = new Dictionary<string, AsyncCommand>();
        protected AsyncCommand GetCommand(Action execute, Func<bool> canExecute, [CallerMemberName] string memberName = null)
        {
            if (memberName == null)
            {
                throw new ArgumentNullException("memberName");
            }
            return _commands.ContainsKey(memberName)
                ? _commands[memberName]
                    : (_commands[memberName] = new AsyncCommand(execute, canExecute));

        }
    }

}

