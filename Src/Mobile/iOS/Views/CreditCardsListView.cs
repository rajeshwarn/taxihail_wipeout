using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using Cirrious.MvvmCross.Binding.BindingContext;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	public partial class CreditCardsListView : BaseViewController<CreditCardsListViewModel>
    {
        private const string Cellid = "CreditCardsCell";
        
        const string CellBindingText = @"
                   LeftText CreditCardDetails.FriendlyName;
                   RightText Last4DigitsWithStars;
                   ShowPlusSign ShowPlusSign;
                   IsFirst IsFirst;
                   IsLast IsLast;
                   Picture Picture;
                   IsAddNewCell IsAddNew;
                   DeleteCommand RemoveCreditCards
                ";
        
        public CreditCardsListView () 
			: base("CreditCardsListView", null)
        {
        }
   
        public override void ViewDidLoad ()
        {
            base.ViewDidLoad ();
            
            View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            tableCardsList.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableCardsList.BackgroundColor = UIColor.Clear;
            
            var source = new BindableCommandTableViewSource(
                tableCardsList, 
                UITableViewCellStyle.Subtitle,
                new NSString(Cellid), 
                CellBindingText,
                UITableViewCellAccessory.None);
            
            source.CellCreator = (tview , iPath, state ) => { 
                return new SingleLinePictureCell( Cellid, CellBindingText ); 
            };

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

            tableCardsList.Source = source;

            NavigationItem.Title = Localize.GetValue("CreditCardsListTitle");
            NavigationItem.BackBarButtonItem = new UIBarButtonItem(Localize.GetValue("BackButton"), UIBarButtonItemStyle.Bordered, null, null);
            View.ApplyAppFont ();
        }   
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;
        }
    }
}


