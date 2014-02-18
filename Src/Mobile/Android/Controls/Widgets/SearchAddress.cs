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

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets
{
    public class SearchAddress: MvxFrameControl
    {    
        private IAppSettings _settings;
        private MvxListView _listAddress;
        private EditText _txtSearchAddress;


        public IEnumerable AllAddresses
        {
            set
            {
                _listAddress.ItemsSource = value;
                // Here, separate in three lists instead of one, with Linq
            }
        }

        public SearchAddress(Context context, IAttributeSet attrs) : base (Resource.Layout.SubView_SearchAddress, context, attrs)
        {
            _settings = TinyIoCContainer.Current.Resolve<IAppSettings>();

            this.DelayBind (() => {
                _listAddress = (MvxListView) FindViewById<MvxListView>(Resource.Id.searchAddressList);
                _txtSearchAddress = (EditText) FindViewById<EditText>(Resource.Id.txtSearchAddress);

                InitializeBinding();


                Observable
                    .FromEventPattern<Android.Text.TextChangedEventArgs>(_txtSearchAddress, "TextChanged")
                    .Throttle(TimeSpan.FromMilliseconds(700))
                    .ObserveOn(SynchronizationContext.Current)
                    .Subscribe(e => ViewModel.TextSearchCommand.Execute(String.Concat(e.EventArgs.Text)));
            });              
        }

        void AddressKeyPress (object sender, KeyEventArgs e)
        {
            
        }

        private AddressPickerViewModel ViewModel { get { return (AddressPickerViewModel)DataContext; } }

        private void InitializeBinding()
        {
            var set = this.CreateBindingSet<SearchAddress, AddressPickerViewModel>();

            set.Bind()
                .For("AllAddresses")
                .To(vm => vm.AllAddresses)
                .OneWay();

            set.Apply();
        }
    }
}

