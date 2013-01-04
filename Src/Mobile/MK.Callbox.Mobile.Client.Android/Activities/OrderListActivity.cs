using Android.App;
using Android.OS;
using Cirrious.MvvmCross.Binding.Android.Views;
using apcurium.MK.Booking.Mobile.ViewModels.Callbox;

namespace apcurium.MK.Callbox.Mobile.Client.Activities
{
    [Activity(Label = "Order List", Theme = "@android:style/Theme.NoTitleBar")]
    public class OrderListActivity : MvxBindingActivityView<CallboxOrderListViewModel>
    {
        protected override void OnViewModelSet()
        {
            SetContentView(Resource.Layout.View_OrderList);
        }
    }
}