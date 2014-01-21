using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Order;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Touch.Views;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class AddressSearchView : MvxViewController
	{
		private const string Cellid = "AdressCell";

		const string CellBindingText = @"
				   FirstLine DisplayLine1;
                   SecondLine DisplayLine2;
				   ShowRightArrow ShowRightArrow;
				   ShowPlusSign ShowPlusSign;
                   Icon Icon;
				   IsFirst IsFirst;
				   IsLast IsLast
				";
        
		public AddressSearchView() 
			: base("AddressSearchView", null)
        {
        }	

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            			
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));            		

			((SearchTextField)SearchTextField).SetImageLeft( "Assets/Search/SearchIcon.png" );
			SearchTextField.Placeholder = Localize.GetValue("searchHint");
            			
            var source = new BindableTableViewSource(
                                AddressListView, 
                                UITableViewCellStyle.Subtitle,
                                new NSString(Cellid), 
                                CellBindingText,
								UITableViewCellAccessory.None);

            source.CellCreator = (tview , iPath, state ) => { return new apcurium.MK.Booking.Mobile.Client.Controls.Widgets.TwoLinesCell( Cellid, CellBindingText, UITableViewCellAccessory.None ); };

 			var set = this.CreateBindingSet<AddressSearchView, AddressSearchViewModel>();
			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.AddressViewModels);
			set.Bind(source)
				.For(v => v.SelectedCommand)
				.To(vm => vm.RowSelectedCommand);

			set.Bind(SearchTextField)
				.For(v => v.Text)
				.To(vm => vm.Criteria);
			set.Bind(SearchTextField)
				.For("IsProgressing")
				.To(vm => vm.IsSearching);

			set.Apply ();

            AddressListView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear };
            AddressListView.Source = source;

            SearchTextField.ReturnKeyType = UIReturnKeyType.Done;
            SearchTextField.ShouldReturn = delegate {
				return SearchTextField.ResignFirstResponder();
            };

            View.ApplyAppFont ();
		}
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
			NavigationItem.Title = Localize.GetValue("View_SearchAddress");
        
            NavigationController.SetViewControllers(NavigationController.ViewControllers.Where ( v=> v.GetType () != typeof(  BookStreetNumberView ) ).ToArray (), false );
        }
	}
}

