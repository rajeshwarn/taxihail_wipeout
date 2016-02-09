using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public class DebugView : MvxTableViewController
    {
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = "Debug";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var source = new MvxStandardTableViewSource(TableView);

            var set = this.CreateBindingSet<DebugView, DebugViewModel>();

            set.Bind(source)
                .For(v => v.ItemsSource)
                .To(vm => vm.DebugEntries);

            set.Apply();

            TableView.Source = source;
            TableView.ReloadData();
        }
    }
}
