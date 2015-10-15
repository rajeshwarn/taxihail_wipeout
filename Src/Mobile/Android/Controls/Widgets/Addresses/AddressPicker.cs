using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Input;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Entity;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Runtime;
using apcurium.MK.Booking.Mobile.Client.Diagnostic;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    [Register("apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses.AddressPicker")]
    public class AddressPicker : MvxFrameControl
    {
        private LinearLayout _searchList;
        private LinearLayout _defaultList;
        private AddressListView _favoriteAddressList;
        private AddressListView _recentAddressList;
        private AddressListView _nearbyAddressList;
        private AddressListView _searchResultsAddressList;    
        private EditText _addressEditText;
        private ScrollView _scrollView;
        private Button _cancelButton;
        private readonly SerialDisposable _collectionChangedSubscription = new SerialDisposable();
        private readonly CompositeDisposable _subscriptions = new CompositeDisposable();
        private AddressPickerViewModel ViewModel 
        { 
            get { return (AddressPickerViewModel)DataContext; } 
        }

        public ICommand SelectedCommand { get; set; }

        public AddressPicker(Context context, IAttributeSet attrs) : base(Resource.Layout.Control_AddressPicker, context, attrs)
        {
            this.DelayBind(() =>
            {
                _searchList = Content.FindViewById<LinearLayout>(Resource.Id.SearchList);                
                _defaultList = Content.FindViewById<LinearLayout>(Resource.Id.DefaultList); 
                _favoriteAddressList = Content.FindViewById<AddressListView>(Resource.Id.FavoriteAddressList); 
                _recentAddressList = Content.FindViewById<AddressListView>(Resource.Id.RecentAddressList); 
                _nearbyAddressList = Content.FindViewById<AddressListView>(Resource.Id.NearbyAddressList); 
                _searchResultsAddressList = Content.FindViewById<AddressListView>(Resource.Id.SearchResultsAddressList);
                _addressEditText = Content.FindViewById<EditText>(Resource.Id.addressEditText); 
                _scrollView = Content.FindViewById<ScrollView>(Resource.Id.scrollView); 
                _cancelButton = Content.FindViewById<Button>(Resource.Id.cancelButton); 

                _addressEditText.SetSelectAllOnFocus(true);

                _addressEditText.OnKeyDown()
                    .Throttle(TimeSpan.FromMilliseconds(700))
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(text => 
                    { 
                        if (_addressEditText.HasFocus)
                        {
                            ExecuteSearchCommand(text);
                        }
                    })
                    .DisposeWith(_subscriptions);

                _addressEditText.EditorAction += (sender, args) =>
                {
                    if (args.ActionId != ImeAction.Go)
                    {
                        return;
                    }
                };

                _scrollView.Touch += (s, e) =>
                {
                    _addressEditText.HideKeyboard();
                    e.Handled = false;
                };

                InitializeBinding();

                _searchResultsAddressList.OnSelectAddress = _nearbyAddressList.OnSelectAddress = _recentAddressList.OnSelectAddress = _favoriteAddressList.OnSelectAddress = address =>
                {                    
                    SelectedCommand.Execute(address);
                };

            });
        }

        private void ExecuteSearchCommand(string text)
        {
            if (ViewModel != null)
            {
                ViewModel.TextSearchCommand.ExecuteIfPossible(text);
            }
        }

        private void ClearAddresses()
        {
            _searchResultsAddressList.Addresses.Clear();
            _favoriteAddressList.Addresses.Clear();
            _recentAddressList.Addresses.Clear();
            _nearbyAddressList.Addresses.Clear();
        }

        private void AddAddresses(IEnumerable<AddressViewModel> addresses)
        {
            if (ViewModel.ShowDefaultResults)
            {
                _favoriteAddressList.Addresses.AddMultiple(addresses.Where(a => a.Type == AddressType.Favorites));
                _recentAddressList.Addresses.AddMultiple(addresses.Where(a => a.Type == AddressType.History));
                _nearbyAddressList.Addresses.AddMultiple(addresses.Where(a => a.Type == AddressType.Places));

                _searchList.Visibility = ViewStates.Gone;
                _defaultList.Visibility = ViewStates.Visible;
            }
            else
            {                
                _searchResultsAddressList.Addresses.AddMultiple(addresses);

                _searchList.Visibility = ViewStates.Visible;
                _defaultList.Visibility = ViewStates.Gone;
            }
        }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<AddressPicker, AddressPickerViewModel>();

            set.Bind()
                .For(v => v.SelectedCommand)
                .To(vm => vm.AddressSelected);

            set.Bind(_addressEditText)
                .For(v => v.Text)
                .To(vm => vm.StartingText)
                .OneWay();

            set.Bind(_cancelButton)
                .For("Click")
                .To(vm => vm.Cancel);

            set.Apply();
        }

		public async void Open(AddressLocationType filterAddresses)
        {
            _collectionChangedSubscription.Disposable = Observable
                .FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => ViewModel.AllAddresses.CollectionChanged += h, 
                    h => ViewModel.AllAddresses.CollectionChanged -= h)
                .ObserveOn(SynchronizationContext.Current)
                .Select(x => x.EventArgs)
                .Subscribe(e => 
                    { 
                        var newItems = new AddressViewModel[0];
                        if(e.NewItems != null)
                        {
                            newItems = e.NewItems.OfType<AddressViewModel>().ToArray();
                        }

                        switch(e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                            {                                    
                                AddAddresses(newItems);
                                break;
                            }
                            case NotifyCollectionChangedAction.Reset:
                            {
                                ClearAddresses();
                                break;
                            }
                            default:
                            {
                                throw new ArgumentOutOfRangeException("Not supported "+ e.Action);
                            }
                        }   
                    }, Logger.LogError);

			await ViewModel.LoadAddresses(filterAddresses).HandleErrors();
			if (filterAddresses == AddressLocationType.Unspeficied || ViewModel.AllAddresses.Count > 1)
            {
                FocusOnTextField();
            }
        } 

        public void Close()
        {
			_addressEditText.HideKeyboard();
			_favoriteAddressList.Collapse();
			_recentAddressList.Collapse();
			_nearbyAddressList.Collapse();

            _collectionChangedSubscription.Disposable = null;
        }

		public void FocusOnTextField()
		{
			_addressEditText.PostDelayed(() => 
			{
				_addressEditText.RequestFocusFromTouch();
				_addressEditText.ShowKeyboard();
				_addressEditText.SetCursorAtEnd();
			}, 400);
		}
 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscriptions.Dispose();
                _collectionChangedSubscription.Disposable = null;
            }

            base.Dispose(disposing);
        }
    }
}