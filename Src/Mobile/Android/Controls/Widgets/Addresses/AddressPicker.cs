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
        private CompositeDisposable _subscriptions = new CompositeDisposable();

        private ObservableCollection<AddressViewModel> _allAddresses;       
        public ObservableCollection<AddressViewModel> AllAddresses
        {
            set
            {
                if (value == null)
                    return;

                _allAddresses = value;

                _searchResultsAddressList.Addresses = _allAddresses;

                _favoriteAddressList.Addresses = new ObservableCollection<AddressViewModel>( value.Cast<AddressViewModel>().Where(x => x.Type == AddressType.Favorites));
                _recentAddressList.Addresses =new ObservableCollection<AddressViewModel>( value.Cast<AddressViewModel>().Where(x => x.Type == AddressType.History));
                _favoriteAddressList.Addresses = new ObservableCollection<AddressViewModel>(value.Cast<AddressViewModel>().Where(x => x.Type == AddressType.Places));
            }

            get
            {
                return _allAddresses;
            }
        }

        bool _showDefaultResults;
        public bool ShowDefaultResults
        {
            get
            {
                return _showDefaultResults;
            }
            set{
                _showDefaultResults = value;

                _searchList.Visibility = value
                    ? ViewStates.Gone
                    : ViewStates.Visible;

                _defaultList.Visibility = value
                    ? ViewStates.Visible
                    : ViewStates.Gone;

                ClearAddresses();
                AddAddresses(ViewModel.AllAddresses);
            }
        }

        public ICommand SelectedCommand { get; set; }

        public AddressPicker(Context context, IAttributeSet attrs)
            : base(Resource.Layout.Control_AddressPicker, context, attrs)
        {

            this.DelayBind(() =>
            {

                _searchList = FindViewById<LinearLayout>(Resource.Id.SearchList); 

                _defaultList= FindViewById<LinearLayout>(Resource.Id.DefaultList); 


                _favoriteAddressList= FindViewById<AddressListView>(Resource.Id.FavoriteAddressList); 


                _recentAddressList= FindViewById<AddressListView>(Resource.Id.RecentAddressList); 


                _nearbyAddressList=FindViewById<AddressListView>(Resource.Id.NearbyAddressList); 


                _searchResultsAddressList= FindViewById<AddressListView>(Resource.Id.SearchResultsAddressList);


                 _addressEditText =             FindViewById<EditText>(Resource.Id.addressEditText); 


                _scrollView= FindViewById<ScrollView>(Resource.Id.scrollView); 


                _cancelButton= FindViewById<Button>(Resource.Id.cancelButton); 


                Observable
                    .FromEventPattern<AfterTextChangedEventArgs>(_addressEditText, "AfterTextChanged")
                    .Where(_ => !ignoreTextChange)
                    .Throttle(TimeSpan.FromMilliseconds(700))
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(e => ViewModel.TextSearchCommand.Execute(String.Concat(e.EventArgs.Editable)))
                    .DisposeWith(_subscriptions);

                _addressEditText.EditorAction += (sender, args) =>
                {
                    if (args.ActionId != ImeAction.Go) return;

                    Close();
                };

                _scrollView.Touch += (s, e) =>
                {
                    _addressEditText.HideKeyboard();
                    e.Handled = false;
                };

                _searchResultsAddressList.HideViewAllButton = true;
                _nearbyAddressList.HideViewAllButton = _recentAddressList.HideViewAllButton = _favoriteAddressList.HideViewAllButton = false;
                _cancelButton.Click += (sender, args) => Close();

                _searchResultsAddressList.OnSelectAddress = _nearbyAddressList.OnSelectAddress = _recentAddressList.OnSelectAddress = _favoriteAddressList.OnSelectAddress = address =>
                {                    
                    SelectedCommand.Execute(address);
                    Close();
                };

                ViewModel.AllAddresses.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => 
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
                };

                InitializeBinding();

            });
        }

        void ClearAddresses()
        {
            _searchResultsAddressList.Addresses.Clear();
            _favoriteAddressList.Addresses.Clear();
            _recentAddressList.Addresses.Clear();
            _nearbyAddressList.Addresses.Clear();
        }

        private void AddAddresses(IEnumerable<AddressViewModel> addresses)
        {
            if (ShowDefaultResults)
            {
                _favoriteAddressList.Addresses.AddMultiple(addresses.Where(a=>a.Type == AddressType.Favorites));
                _recentAddressList.Addresses.AddMultiple(addresses.Where(a=>a.Type == AddressType.History));
                _nearbyAddressList.Addresses.AddMultiple(addresses.Where(a=>a.Type == AddressType.Places));
            }
            else
            {                
                _searchResultsAddressList.Addresses.AddMultiple(addresses);
            }

        }

        private void AddRemove(AddressViewModel address)
        {
            _searchResultsAddressList.Addresses.Remove(address);
            _favoriteAddressList.Addresses.Remove(address);
            _recentAddressList.Addresses.Remove(address);
            _nearbyAddressList.Addresses.Remove(address);
        }


        private AddressPickerViewModel ViewModel { get { return (AddressPickerViewModel)DataContext; } }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<AddressPicker, AddressPickerViewModel>();

            set.Bind()
                .For(v => v.SelectedCommand)
                .To(vm => vm.AddressSelected);

            set.Bind()
                .For(v => v.ShowDefaultResults)
                .To(vm => vm.ShowDefaultResults);

            set.Apply();
        }

        private LinearLayout _searchList;

        private LinearLayout _defaultList;

        private AddressListView _favoriteAddressList;
       
        private AddressListView _recentAddressList;

        private AddressListView _nearbyAddressList;

        private AddressListView _searchResultsAddressList;    

        private EditText _addressEditText;

        private ScrollView _scrollView;

        private Button _cancelButton;

        public void Open()
        {
        
            ViewModel.LoadAddresses(
//                ()=>{
//
//                AddressEditText.Text = GetFirstPortionOfAddress(startingAddress.DisplayAddress);
//                AddressEditText.SelectAll();
//
//
//            }
            );

            //_callback = callback;



            Visibility = ViewStates.Visible;
        } 

        private Action<AddressViewModel> _callback;

        private void Close()
        {
            Visibility = ViewStates.Gone;
            _addressEditText.HideKeyboard();
            _favoriteAddressList.Collapse();
            _recentAddressList.Collapse();
            _nearbyAddressList.Collapse();

        }

        bool ignoreTextChange = false;

        private string GetFirstPortionOfAddress( string fullAddress )
        {
            if ( (fullAddress.HasValue()) && ( fullAddress.Contains( "," ) ) )
            {
                return fullAddress.Split(',')[0];
            }
            else
            {
                return fullAddress;
            }

        }
            
        public void Dispose()
        {
            _subscriptions.Dispose();
        }
    }
}