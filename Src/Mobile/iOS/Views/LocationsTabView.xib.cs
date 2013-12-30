
using System;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.ViewModels;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using TinyIoC;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Client.Extensions;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Threading.Tasks;
using System.Threading;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;

namespace apcurium.MK.Booking.Mobile.Client
{
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

		private CancellationTokenSource _searchCancellationToken = new CancellationTokenSource();
		public event EventHandler Canceled;
		public event EventHandler LocationSelected;
		#region Constructors

        public LocationsTabView () 
            : base(new MvxShowViewModelRequest<MyLocationsViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
            Initialize();
        }
        
        public LocationsTabView (MvxShowViewModelRequest request) 
            : base(request)
        {
            Initialize();
        }
        
        public LocationsTabView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
            Initialize();
        }

        #endregion
        
        
        void Initialize()
        {
        }

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));

            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_LocationList"), true);

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
            this.View.ApplyAppFont ();
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}

	}
}

