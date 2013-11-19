using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.Views;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using Cirrious.MvvmCross.Binding.Touch.Interfaces;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using System.Linq;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using System.Collections.Generic;
using Cirrious.MvvmCross.Binding.Interfaces;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;
using MonoTouch.AddressBook;
using Xamarin.Contacts;
using TinyIoC;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class AddressSearchView : MvxBindingTouchViewController<AddressSearchViewModel>
	{
		private const string CELLID = "AdressCell";

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
            : base(new MvxShowViewModelRequest<AddressSearchViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
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

     
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
            			
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));            		

			((SearchTextField)SearchTextField).SetImageLeft( "Assets/Search/SearchIcon.png" );
			SearchTextField.Placeholder = Resources.SearchHint;
            			
			var source = new BindableAddressTableViewSource(
                                AddressListView, 
                                UITableViewCellStyle.Subtitle,
                                new NSString(CELLID), 
                                CellBindingText,
								UITableViewCellAccessory.None);

			source.CellCreator = (tview , iPath, state ) => { return new TwoLinesCell( CELLID, CellBindingText ); };

            this.AddBindings(new Dictionary<object, string>(){			
                {source, "{'ItemsSource':{'Path':'AddressViewModels'}, 'SelectedCommand':{'Path':'RowSelectedCommand'}}"} ,				
                {SearchTextField, "{'Text':{'Path':'Criteria'}, 'IsProgressing':{'Path':'IsSearching'}}"} ,
			});

            AddressListView.BackgroundView = new UIView{ BackgroundColor = UIColor.Clear };
            AddressListView.Source = source;

            SearchTextField.ReturnKeyType = UIReturnKeyType.Done;
            SearchTextField.ShouldReturn = delegate(UITextField textField)
            {
				return SearchTextField.ResignFirstResponder();
            };

            this.View.ApplyAppFont ();
		}
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_SearchAddress"), true);
        
            this.NavigationController.SetViewControllers(this.NavigationController.ViewControllers.Where ( v=> v.GetType () != typeof(  BookStreetNumberView ) ).ToArray (), false );
        }

		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
//
//		public string GetTitle()
//        {
//            return "";
//        }
//        
//      	public bool IsTopView
//        {
//            get { return this.NavigationController.TopViewController is AddressSearchView; }
//        }
//
//        public UIView GetTopView()
//        {
//            return null;
//        }
	}
}

