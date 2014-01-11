using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.ViewModels;
using Cirrious.MvvmCross.Views;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
	//TODO [MvvMCross v3] Required? 
	[MvxViewFor(typeof(MyLocationsViewModel))]
	public partial class LocationsTabView : BaseViewController<MyLocationsViewModel>
	{
        const string CellBindingText = @"
                {
                   'FirstLine':{'Path':'Address.FriendlyName'},
                   'SecondLine':{'Path':'Address.FullAddress'},
                   'ShowRightArrow':{'Path':'ShowRightArrow'},
                   'ShowPlusSign':{'Path':'ShowPlusSign'},
                   'IsFirst':{'Path':'IsFirst'},
                   'IsLast':{'Path':'IsLast'},
                    'Icon':{'Path':'Icon'},
                   'IsAddNewCell': {'Path': 'IsAddNew'}
                }";

        public LocationsTabView () 
			: base("LocationsTabView", null)
        {
            Initialize();
        }
        
        void Initialize()
        {
        }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_LocationList"), true);

			tableLocations.SectionHeaderHeight = 33;
            tableLocations.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableLocations.BackgroundColor = UIColor.Clear;

            var source = new BindableAddressTableViewSource(
                tableLocations, 
                UITableViewCellStyle.Subtitle,
                new NSString("LocationCell"), 
                CellBindingText,
                UITableViewCellAccessory.None);
            
            source.CellCreator = (tview , iPath, state ) => { return new TwoLinesCell( new NSString("LocationCell"), CellBindingText ); };

            this.AddBindings(new Dictionary<object, string>{
                { source, "{'ItemsSource': {'Path': 'AllAddresses'}, 'SelectedCommand': {'Path': 'NavigateToLocationDetailPage'}}" },
            });

            tableLocations.Source = source;
            View.ApplyAppFont ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}

	}
}

