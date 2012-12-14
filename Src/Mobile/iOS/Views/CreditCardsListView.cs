
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Touch.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class CreditCardsListView :  MvxBindingTouchViewController<CreditCardsListViewModel>
    {

        private const string CELLID = "CreditCardsCell";
        
        const string CellBindingText = @"
                {
                   'FirstLine':{'Path':'Title'},
                   'SecondLine':{'Path':'PickupAddress.BookAddress'},
                   'ShowRightArrow':{'Path':'ShowRightArrow'},
                   'ShowPlusSign':{'Path':'ShowPlusSign'},
                   'IsFirst':{'Path':'IsFirst'},
                   'IsLast':{'Path':'IsLast'}
                }";

        #region Constructors
        
        public CreditCardsListView () 
            : base(new MvxShowViewModelRequest<CreditCardsListViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
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

            NavigationItem.HidesBackButton = false;

            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_CreditCardsList"), true);
            
            lblInfo.Text = Resources.HistoryInfo;   
            lblInfo.TextColor = AppStyle.TitleTextColor;
            lblNoHistory.Text = Resources.NoHistoryLabel;
            tableCreditCards.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableCreditCards.BackgroundColor = UIColor.Clear;
            
            var source = new BindableCommandTableViewSource(
                tableCreditCards, 
                UITableViewCellStyle.Subtitle,
                new NSString(CELLID), 
                CellBindingText,
                UITableViewCellAccessory.None);
            
            source.CellCreator = (tview , iPath, state ) => { 
                return new TwoLinesCell( CELLID, CellBindingText ); 
            };
            this.AddBindings(new Dictionary<object, string>(){
                {tableCreditCards, "{'Hidden': {'Path': 'HasOrders', 'Converter': 'BoolInverter'}}"},
                {lblNoHistory, "{'Hidden': {'Path': 'HasOrders'}}"},
                {source, "{'ItemsSource':{'Path':'Orders'}, 'SelectedCommand':{'Path':'NavigateToHistoryDetailPage'}}"} ,
            });
            
            tableHistory.Source = source;

            //var add = new UIBarButtonItem(UIBarButtonSystemItem.Add, null, null);
            //add.Clicked += (sender, e) => ViewModel.NavigateToAddCreditCard.Execute();
            //NavigationItem.RightBarButtonItem = add;

            //this.AddBindings(new Dictionary<object, string>(){ });
            //NavigationItem.Title = Resources.GetValue( "CreditCardsListTitle");
            this.View.ApplyAppFont ();
        }	

    }
}

