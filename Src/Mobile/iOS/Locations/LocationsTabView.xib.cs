
using System;
using System.Collections.Generic;
using System.Linq;
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
	public enum LocationsTabViewMode
	{
		Edit,
		FavoritesSelector,
		NearbyPlacesSelector
	}
    public partial class LocationsTabView : MvxBindingTouchViewController<MyLocationsViewModel>
	{
        const string CellBindingText = @"
                {
                   'FirstLine':{'Path':'Address.FriendlyName'},
                   'SecondLine':{'Path':'Address.FullAddress'},
                   'ShowRightArrow':{'Path':'ShowRightArrow'},
                   'ShowPlusSign':{'Path':'ShowPlusSign'},
                   'IsFirst':{'Path':'IsFirst'},
                   'IsLast':{'Path':'IsLast'},
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
            Mode = LocationsTabViewMode.Edit;
        }

		public LocationsTabViewMode Mode { get; set; }


		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			if (Mode == LocationsTabViewMode.Edit) {
				View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			} 
			else
			{				
				View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
				NavigationItem.RightBarButtonItem = new UIBarButtonItem( UIBarButtonSystemItem.Cancel , CanceledTouchUpInside );			
			}

            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_LocationList"), true);


			tableLocations.SectionHeaderHeight = 33;

            tableLocations.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
            tableLocations.BackgroundColor = UIColor.Clear;
			//LoadGridData ();

            var source = new BindableAddressTableViewSource(
                tableLocations, 
                UITableViewCellStyle.Subtitle,
                new NSString("LocationCell"), 
                CellBindingText,
                UITableViewCellAccessory.None);
            
            source.CellCreator = (tview , iPath, state ) => { return new TwoLinesCell( new NSString("LocationCell"), CellBindingText ); };

            this.AddBindings(new Dictionary<object, string>{
                { source, "{'ItemsSource': {'Path': 'AllAddresses'}}" },
            });

            tableLocations.Source = source;
            ViewModel.OnViewLoaded();
	
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			NavigationController.NavigationBar.Hidden = false;
		}

		void CanceledTouchUpInside (object sender, EventArgs e)
		{
			if (Canceled != null) {
				Canceled (this, EventArgs.Empty);
			}
		}

		/*private void LoadGridData ()
		{
			if (tableLocations == null) {
				return;
			}
			TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(true, () => CancelCurrentTask() );
			_searchCancellationToken = new CancellationTokenSource();
			var task = new Task<InfoStructure>(() => GetLocationsStructure(), _searchCancellationToken.Token);
			task.ContinueWith(RefreshData);
			task.Start();
		}*/

		public void RefreshData( Task<InfoStructure>  task )
		{
			if(task.IsCompleted && !task.IsCanceled)
			{
				InvokeOnMainThread( () => {
					tableLocations.DataSource = new LocationTableViewDataSource (this, task.Result);
					tableLocations.Delegate = new LocationTableViewDelegate (this, task.Result);
					tableLocations.ReloadData ();
                    TinyIoCContainer.Current.Resolve<IMessageService>().ShowProgress(false);
				});
			}
			
		}

		private void CancelCurrentTask()
		{
			if (_searchCancellationToken != null
			    && _searchCancellationToken.Token.CanBeCanceled)
			{
				_searchCancellationToken.Cancel();
				_searchCancellationToken.Dispose();
				_searchCancellationToken = null;
			}
		}

		public List<Address> LocationList { get; set; }

		/*private List<Address> GetFavorites()
		{
			List<Address> favorites = new List<Address>();
            var adrs =TinyIoCContainer.Current.Resolve<IAccountService>().GetFavoriteAddresses();
            if ( adrs.Count() > 0 )
            {
                favorites.AddRange( adrs );
            }
			
			if ( Mode == LocationsTabViewMode.Edit )
			{
                favorites.Add (new Address{ FriendlyName=Resources.GetValue("LocationAddFavoriteTitle"), FullAddress = Resources.GetValue("LocationAddFavoriteSubtitle")} );
			}
			return favorites;
		}

		private List<Address> GetHistoric()
		{
			List<Address> historics = new List<Address>();
            var adrs =TinyIoCContainer.Current.Resolve<IAccountService>().GetHistoryAddresses();
            if ( adrs.Count() > 0 )
            {
                adrs.ForEach( a=> a.IsHistoric = true);
                historics.AddRange( adrs );
            }
            return historics;
		}*/


		public void DoSelect ( Address data )
		{
			SelectedLocation =data;
			if ( LocationSelected != null )
			{
				LocationSelected( this, EventArgs.Empty );
			}
		}

		public Address SelectedLocation { get; set; }

		/*public void Delete (Address data)
		{			
			if( data.IsHistoric )
			{
				TinyIoCContainer.Current.Resolve<IAccountService>().DeleteHistoryAddress( data.Id );
			}
			else
			{
            	TinyIoCContainer.Current.Resolve<IAccountService>().DeleteFavoriteAddress( data.Id );
			}
			LoadGridData ();
		}

		public void Update (Address data)
		{
            TinyIoCContainer.Current.Resolve<IAccountService>().UpdateAddress( data );     
            data.IsHistoric= false;
            LoadGridData ();
		}*/

		
		/*private InfoStructure GetLocationsStructure()
		{
			var structure = new InfoStructure( 44, false );

			
			var favorites = GetFavorites();
			var historic = GetHistoric();

			var sectFav = structure.AddSection( Resources.FavoriteLocationsTitle );

			sectFav.SectionLabelTextColor = AppStyle.TitleTextColor.ToArray();
			sectFav.EditMode = Mode == LocationsTabViewMode.Edit;
			favorites.ForEach( item => sectFav.AddItem( new TwoLinesAddressItem( item.Id,  item.FriendlyName, item.FullAddress ) { Data = item, ShowRightArrow = Mode == LocationsTabViewMode.Edit && !item.Id.IsNullOrEmpty(), ShowPlusSign = item.Id.IsNullOrEmpty() } ) );

			var sectHist = structure.AddSection( Resources.LocationHistoryTitle );
			sectHist.SectionLabelTextColor = AppStyle.TitleTextColor.ToArray();
			sectHist.EditMode = Mode == LocationsTabViewMode.Edit;
            historic.ForEach( item => sectHist.AddItem( new TwoLinesAddressItem( item.Id,    item.FriendlyName, item.FullAddress ) { Data = item, ShowRightArrow = Mode == LocationsTabViewMode.Edit && !item.Id.IsNullOrEmpty(), Enabled = () => !item.Id.IsNullOrEmpty() } ) );


			return structure;
		}*/
	}
}

