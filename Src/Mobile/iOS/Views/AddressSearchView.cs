using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Client.Order;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class AddressSearchView : MvxBindingTouchViewController<AddressSearchViewModel>
	{
		private const string Cellid = "AdressCell";

		const string CellBindingText = @"
                {
                   'FirstLine':{'Path':'DisplayLine1'},
                   'SecondLine':{'Path':'DisplayLine2'},
				   'ShowRightArrow':{'Path':'ShowRightArrow'},
				   'ShowPlusSign':{'Path':'ShowPlusSign'},
                   'Icon':{'Path':'Icon'},
				   'IsFirst':{'Path':'IsFirst'},
				   'IsLast':{'Path':'IsLast'},
				}";
		#region Constructors
        public AddressSearchView() 
            : base(new MvxShowViewModelRequest<AddressSearchViewModel>( null, true, new MvxRequestedBy()   ) )
        {
        }

        public AddressSearchView(MvxShowViewModelRequest request) 
            : base(request)
        {
        }

        public AddressSearchView(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }	
		#endregion

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            			
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));            		

			((SearchTextField)SearchTextField).SetImageLeft( "Assets/Search/SearchIcon.png" );
			SearchTextField.Placeholder = Localize.GetValue("searchHint");
            			
			var source = new BindableAddressTableViewSource(
                                AddressListView, 
                                UITableViewCellStyle.Subtitle,
                                new NSString(Cellid), 
                                CellBindingText,
								UITableViewCellAccessory.None);

			source.CellCreator = (tview , iPath, state ) => { return new TwoLinesCell( Cellid, CellBindingText ); };

            this.AddBindings(new Dictionary<object, string>{			
                {source, "{'ItemsSource':{'Path':'AddressViewModels'}, 'SelectedCommand':{'Path':'RowSelectedCommand'}}"} ,				
                {SearchTextField, "{'Text':{'Path':'Criteria'}, 'IsProgressing':{'Path':'IsSearching'}}"} ,
			});

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
            NavigationItem.TitleView = new TitleView(null, Localize.GetValue("View_SearchAddress"), true);
        
            NavigationController.SetViewControllers(NavigationController.ViewControllers.Where ( v=> v.GetType () != typeof(  BookStreetNumberView ) ).ToArray (), false );
        }
	}
}

