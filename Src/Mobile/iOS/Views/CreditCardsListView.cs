using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardsListView : BaseViewController<CreditCardsListViewModel>
    {
        private const string Cellid = "CreditCardsCell";
        const string CellBindingText = @"
                   LeftText CreditCardDetails.FriendlyName;
                   RightText Last4DigitsWithStars;
                   IsAddNewCell IsAddNew;
                   Icon Picture;
                   DeleteCommand RemoveCreditCards
                ";
        
        public CreditCardsListView () : base("CreditCardsListView", null)
        {
        }
   
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);

            NavigationController.NavigationBar.Hidden = false;
            NavigationItem.Title = Localize.GetValue ("CreditCardsListView");
        }

        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            
            View.BackgroundColor = UIColor.FromRGB (239, 239, 239);

            tableCardsList.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableCardsList.BackgroundColor = UIColor.Clear;
            tableCardsList.SeparatorColor = UIColor.Clear;
            
            var source = new BindableTableViewSource(
                tableCardsList, 
                UITableViewCellStyle.Subtitle,
                new NSString(Cellid), 
                CellBindingText,
                UITableViewCellAccessory.None);
            
            source.CellCreator = CellCreator;
            tableCardsList.Source = source;

			var set = this.CreateBindingSet<CreditCardsListView, CreditCardsListViewModel>();

			set.Bind(tableCardsList)
				.For(v => v.Hidden)
				.To(vm => vm.HasCards)
				.WithConversion("BoolInverter");

			set.Bind(source)
				.For(v => v.ItemsSource)
				.To(vm => vm.CreditCards);
			set.Bind(source)
				.For(v => v.SelectedCommand)
				.To(vm => vm.NavigateToAddOrSelect);

			set.Apply();
        }   

        private MvxStandardTableViewCell CellCreator(UITableView tableView, NSIndexPath indexPath, object state)
        {
            var cell = new SingleLinePictureCell( Cellid, CellBindingText );
            cell.HideBottomBar = tableView.IsLastCell(indexPath);
            return cell;
        }
    }
}


