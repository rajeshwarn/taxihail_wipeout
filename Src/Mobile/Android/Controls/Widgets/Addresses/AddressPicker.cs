using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using System.Collections;
using System.Reactive.Linq;
using System.Threading;
using apcurium.MK.Booking.Mobile.Extensions;
using System.Reactive.Disposables;
using System.Linq;
using Android.Text;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using Android.Views.InputMethods;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Entity;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Windows.Input;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
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

        private CompositeDisposable _subscriptions = new CompositeDisposable();
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
                    .Subscribe(text => ExecuteSearchCommand(text));

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
            if (ViewModel != null && ViewModel.TextSearchCommand != null)
            {
                ViewModel.TextSearchCommand.Execute(text);
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
            ViewModel.AllAddresses.CollectionChanged += (sender, e) => {

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
            };

            var set = this.CreateBindingSet<AddressPicker, AddressPickerViewModel>();

            set.Bind()
                .For(v => v.SelectedCommand)
                .To(vm => vm.AddressSelected);

            set.Bind(_addressEditText)
                .For(v => v.Text)
                .To(vm => vm.StartingText);

            set.Bind(_cancelButton)
                .For("Click")
                .To(vm => vm.Cancel);

            set.Apply();
        }

        public void Open()
        {        
            Visibility = ViewStates.Visible;
        } 

        public void Close()
        {
            Visibility = ViewStates.Gone;
            _addressEditText.HideKeyboard();
            _favoriteAddressList.Collapse();
            _recentAddressList.Collapse();
            _nearbyAddressList.Collapse();
        }
 
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscriptions.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}