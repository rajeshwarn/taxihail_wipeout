using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.ViewModels.Payment;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class CreditCardsListView :  BaseViewController<CreditCardsListViewModel>
    {
        
        private const string Cellid = "CreditCardsCell";
        
        const string CellBindingText = @"
                {
                   'LeftText':{'Path':'CreditCardDetails.FriendlyName'} ,
                   'RightText':{'Path':'Last4DigitsWithStars'} ,
                   'ShowPlusSign':{'Path':'ShowPlusSign'} ,
                   'IsFirst':{'Path':'IsFirst'} ,
                   'IsLast':{'Path':'IsLast'},
                   'Picture':{'Path':'Picture'},
                   'IsAddNewCell':{'Path':'IsAddNew'},
                   'DeleteCommand':{'Path':'RemoveCreditCards'}
                }";
        
        #region Constructors
        
        public CreditCardsListView () 
            : base(new MvxShowViewModelRequest<CreditCardsListViewModel>( null, true, new MvxRequestedBy()   ) )
        {
        }
        
        public CreditCardsListView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public CreditCardsListView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }
        
#endregion
        
        
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
            this.AddBindings(new Dictionary<object, string>{
                {tableCardsList, "{'Hidden': {'Path': 'HasCards', 'Converter': 'BoolInverter'}}"} ,
                {source, "{'ItemsSource':{'Path':'CreditCards'}, 'SelectedCommand':{'Path':'NavigateToAddOrSelect'}}"}  ,
            });
            
            tableCardsList.Source = source;

            NavigationItem.Title = Localize.GetValue( "CreditCardsListTitle");
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


