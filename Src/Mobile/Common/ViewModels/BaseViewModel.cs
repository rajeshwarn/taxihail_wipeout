using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Common.Diagnostic;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.Platform;
using Cirrious.MvvmCross.ViewModels;
using MK.Common.Configuration;
using TinyIoC;
using TinyMessenger;
using System.Reactive.Subjects;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace apcurium.MK.Booking.Mobile.ViewModels
{
    public class BaseViewModel: MvxViewModel, IDisposable
    {
		private readonly IList<Tuple<BaseViewModel, bool>> _childViewModels = new List<Tuple<BaseViewModel, bool>>();
		private readonly IList<Func<IDisposable>> _disposableFactories = new List<Func<IDisposable>>();
		private readonly CompositeDisposable _factorySubsciptions = new CompositeDisposable();

		protected readonly CompositeDisposable Subscriptions = new CompositeDisposable();

		public BaseViewModel Parent { get; set; }

		protected new void RaisePropertyChanged([CallerMemberName]string whichProperty = null)
		{
			base.RaisePropertyChanged(whichProperty);
		}

		public virtual void OnViewStarted(bool firstTime)
		{
			foreach (var cvm in _childViewModels.Where(x => x.Item2))
			{
				cvm.Item1.OnViewStarted(firstTime);
			}
			foreach (var factory in _disposableFactories)
			{
				_factorySubsciptions.Add(factory.Invoke());
			}
		}

		public virtual void OnViewStopped()
		{
			foreach (var cvm in _childViewModels.Where(x => x.Item2))
			{
				cvm.Item1.OnViewStopped();
			}
			_factorySubsciptions.Clear();
		}

        protected void Observe<T>(IObservable<T> observable, Action<T> onNext)
        {
			// Buffer to hold the last value produce while VM is stopped
			var buffer = new FixedSizedQueue<T>(1);

			// subject used to record values produced while VM is stopped
			var recordingSubject = new Subject<T>();

			var offline = recordingSubject.Publish();
			offline.Subscribe(x => buffer.Enqueue(x));
			var offlineSubscription = offline.Connect();

			// Start recording now 
			observable.Subscribe(recordingSubject);
			// subject used to record values produced while VM is started
			var liveSubject = new Subject<T>();
			observable.Subscribe(liveSubject);

			var connection = Observable.Create<T>(o =>
			{
				// Stop recording offline values;
				offlineSubscription.Dispose();

				// Produce an observable sequence starting with the last value
				var subscription = liveSubject
					.StartWith(buffer.ToArray())
					.Subscribe(o);

				buffer.Clear();

				// This will reconnect to this offline observable when subscription is disposed
				var goOffline = Disposable.Create(() => {
					offlineSubscription = offline.Connect();
				});

				return new CompositeDisposable(new IDisposable[] {
					subscription,
					goOffline
				});


			}).Publish();

			connection
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(onNext,Logger.LogError);

			_disposableFactories.Add(() => connection.Connect());
        }

		public void Dispose()
		{
			Dispose(true);
		}

		protected TinyIoCContainer Container
		{
			get { return TinyIoCContainer.Current; }
		}

		protected ILogger Logger { get { return Container.Resolve<ILogger>(); } }

		public TaxiHailSetting Settings { get { return Container.Resolve<IAppSettings>().Data; } }

		protected virtual void Dispose(bool disposing)
		{
			if (disposing) 
			{
				Subscriptions.Dispose();
			}
		}

        protected bool ShowSubViewModel<TViewModel, TResult>(object parameterValuesObject, Action<TResult> onResult)
            where TViewModel : MvxViewModel, ISubViewModel<TResult>
        {
            return ShowSubViewModel<TViewModel, TResult>(parameterValuesObject.ToSimplePropertyDictionary(), onResult);
        }

        protected bool ShowSubViewModel<TViewModel, TResult>(IDictionary<string, string> parameterValues, Action<TResult> onResult)
            where TViewModel : MvxViewModel, ISubViewModel<TResult>
        {
            parameterValues = parameterValues ?? new Dictionary<string, string>();

	        if (parameterValues.ContainsKey("messageId"))
	        {
				throw new ArgumentException("parameterValues cannot contain an item with the key 'messageId'");
	        }

            var messageId = Guid.NewGuid().ToString();
            parameterValues["messageId"] = messageId;
            TinyMessageSubscriptionToken token = null;
            // ReSharper disable once RedundantAssignment
            token = this.Services().MessengerHub.Subscribe<SubNavigationResultMessage<TResult>>(msg =>
            {
                // ReSharper disable AccessToModifiedClosure
	            if (token != null)
	            {
					this.Services().MessengerHub.Unsubscribe<SubNavigationResultMessage<TResult>>(token);
	            }
                // ReSharper restore AccessToModifiedClosure

                onResult(msg.Result);
            },
                msg => msg.MessageId == messageId);

            return ShowViewModel<TViewModel>(parameterValues);
        }

        protected void ShowViewModelAndRemoveFromHistory<TViewModel>(object parameter = null) where TViewModel : IMvxViewModel
        {
            var dictionary = parameter.ToSimplePropertyDictionary();
            dictionary = dictionary ?? new Dictionary<string,string>();
            dictionary.Add("removeFromHistory", "notUsed");
            base.ShowViewModel<TViewModel>(dictionary);
        }

        protected void ShowViewModelAndClearHistory<TViewModel>(object parameter = null) where TViewModel : IMvxViewModel
        {
            var dictionary = parameter.ToSimplePropertyDictionary();
            dictionary = dictionary ?? new Dictionary<string, string>();
            dictionary.Add("clearNavigationStack", "notUsed");
            base.ShowViewModel<TViewModel>(dictionary);
        }

		protected void GoBackToHomeViewModel(object parameter)
		{
			var dictionary = parameter.ToSimplePropertyDictionary();
			dictionary = dictionary ?? new Dictionary<string,string>();
			dictionary.Add("clearHistoryExceptFirstElement", "notUsed"); // iOS only: this will not actually cause a navigation.
			dictionary.Add("removeFromHistory", "notUsed"); //Android
			base.ShowViewModel<HomeViewModel>(dictionary);
		}

		protected TViewModel AddChild<TViewModel>(Func<TViewModel> builder) where TViewModel: BaseViewModel
		{
		    var viewModel = builder.Invoke();
			viewModel.Parent = this;
			viewModel.DisposeWith(Subscriptions);

			// from amp, always set to true here
			var forwardParentLifecycleEvents = true; 
			_childViewModels.Add(Tuple.Create<BaseViewModel, bool>(viewModel, forwardParentLifecycleEvents));

			return viewModel;
		}

		protected virtual TViewModel AddChild<TViewModel>() where TViewModel: BaseViewModel
		{
			return AddChild(Mvx.IocConstruct<TViewModel>);
		}

		public override void Start()
		{
			base.Start();
			foreach (var child in _childViewModels)
			{
				try
				{
					child.Item1.Start();
				}
				catch(Exception e)
				{
					Logger.LogError(e);
				}
			}
		}

		protected override void InitFromBundle(IMvxBundle parameters)
		{
			base.InitFromBundle(parameters);
			if(parameters.Data.ContainsKey("messageId"))
			{
				this.MessageId = parameters.Data["messageId"];
			}

			foreach (var child in _childViewModels)
			{
				try
				{
					child.Item1.CallBundleMethods("Init", parameters);
				}
				catch(Exception e)
				{
					Logger.LogError(e);
				}
			}
		}

		public string MessageId { get; private set; }
    }

	public class FixedSizedQueue<T>: IEnumerable<T>
	{
		ConcurrentQueue<T> _innerQueue = new ConcurrentQueue<T>();

		public FixedSizedQueue(int limit)
		{
			Limit = limit;
		}

		public int Limit { get; private set; }

		public void Enqueue(T obj)
		{
			_innerQueue.Enqueue(obj);
			lock (this)
			{
				T overflow;
				while (_innerQueue.Count > Limit && _innerQueue.TryDequeue(out overflow)) ;
			}
		}

		public bool TryDequeue(out T result)
		{
			return _innerQueue.TryDequeue(out result);
		}

		public bool TryPeek(out T result)
		{
			return _innerQueue.TryPeek(out result);
		}

		public void Clear()
		{
			_innerQueue = new ConcurrentQueue<T>();
		}

		public int Count{ get { return _innerQueue.Count; } }
		public bool IsEmpty { get { return _innerQueue.IsEmpty; } }

		public IEnumerator<T> GetEnumerator()
		{
			return _innerQueue.GetEnumerator();
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _innerQueue.GetEnumerator();
		}
	}
}

