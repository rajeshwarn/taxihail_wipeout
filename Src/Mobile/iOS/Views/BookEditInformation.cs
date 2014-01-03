
using System;
using System.Drawing;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.Extensions;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Touch.ExtensionMethods;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Infrastructure;
using System.Reactive.Linq;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public partial class BookEditInformation : BaseViewController<BookEditInformationViewModel>
    {
        public BookEditInformation(MvxShowViewModelRequest request) 
            : base(request)
        {
        }
        
        public BookEditInformation(MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
        }

        public override void LoadView()
        {
            base.LoadView();
            var appSettings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            var isThriev = appSettings.ApplicationName == "Thriev";
            if (isThriev)
            {
                NSBundle.MainBundle.LoadNib ("BookEditInformation_Thriev", this, null);
            } else {
                NSBundle.MainBundle.LoadNib ("BookEditInformation", this, null);
            }
        }
        
        public override void ViewWillAppear (bool animated)
        {
            base.ViewWillAppear (animated);
            NavigationController.NavigationBar.Hidden = false;

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            ViewModel.Load();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            NavigationItem.HidesBackButton = false;           
                        

            lblName.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            txtName.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerName);
            
            lblPassengers.Maybe( x=> x.Hidden = !ViewModel.ShowPassengerNumber);
            txtNbPassengers.Maybe(x=> x.Hidden = !ViewModel.ShowPassengerNumber);
            
            lblPhone.Maybe(x => x.Hidden = !ViewModel.ShowPassengerPhone);
            txtPhone.Maybe(x => x.Hidden = !ViewModel.ShowPassengerPhone);

            contentStack.Maybe(x => x.VerticalAlignement = StackPanelAlignement.Top);
            contentStack.Maybe(x => x.TopOffset = 10);


            lblVehiculeType.Maybe(x => x.Text = Resources.ConfirmVehiculeTypeLabel);
            lblChargeType.Maybe(x => x.Text = Resources.ChargeTypeLabel);                     
            lblEntryCode.Maybe(x => x.Text = Resources.GetValue ( "EntryCodeLabel" ));
            lblApartment.Maybe(x => x.Text = Resources.GetValue ( "ApartmentLabel" ));
            lblName.Maybe(x => x.Text = Resources.GetValue ( "PassengerNameLabel" ));
            lblPassengers.Maybe(x => x.Text = Resources.GetValue ( "PassengerNumberLabel" ));
            lblLargeBags.Maybe(x => x.Text = Resources.GetValue ( "LargeBagsLabel" ));
            lblPhone.Maybe(x => x.Text = Resources.GetValue ( "PassengerPhoneLabel" ));


            scrollView.ContentSize = new SizeF( 320, 700 );

            txtAprtment.Maybe(x => x.Ended += HandleTouchDown);
            txtEntryCode.Maybe(x => x.Ended += HandleTouchDown);

            pickerVehicleType.Configure(Resources.RideSettingsVehiculeType, 
                                        ()=> ViewModel.Vehicles, 
                                        ViewModel.VehicleTypeId, 
                                        x => ViewModel.SetVehicleTypeId(x.Id), 
                                        ViewModel.OnPropertyChanged().Where( property => property == "Vehicles") );

            pickerChargeType.Configure(Resources.RideSettingsChargeType,
                                       ()=>ViewModel.Payments, 
                                       ViewModel.ChargeTypeId, 
                                       x => ViewModel.SetChargeTypeId(x.Id), 
                                       ViewModel.OnPropertyChanged().Where( property => property == "Payments"));

            var bindings = new [] {
                Tuple.Create<object,string>(txtName, "{'Text':{'Path':'Order.Settings.Name'}}"),
                Tuple.Create<object,string>(txtPhone, "{'Text':{'Path':'Order.Settings.Phone'}}"),
                Tuple.Create<object,string>(txtNbPassengers, "{'Text':{'Path':'Order.Settings.Passengers'}}"),
                Tuple.Create<object,string>(txtLargeBags, "{'Text':{'Path':'Order.Settings.LargeBags'}}"),
                Tuple.Create<object,string>(txtAprtment, "{'Text':{'Path':'Order.PickupAddress.Apartment'}}"),
                Tuple.Create<object,string>(txtEntryCode, "{'Text':{'Path':'Order.PickupAddress.RingCode'}}"),
                Tuple.Create<object,string>(pickerVehicleType, "{'Text':{'Path':'VehicleName'}}"),
                Tuple.Create<object,string>(pickerChargeType, "{'Text':{'Path':'ChargeType'}}")
            }
            .Where(x=> x.Item1 != null )
            .ToDictionary(x=>x.Item1, x=>x.Item2);

            this.AddBindings(bindings);
              
            View.ApplyAppFont ();
        }

        void HandleTouchDown (object sender, EventArgs e)
        {
            txtAprtment.Maybe(x => x.ResignFirstResponder ());
            txtEntryCode.Maybe(x => x.ResignFirstResponder ());
         
        }
        
        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            NavigationItem.TitleView = new TitleView(null, Resources.View_BookingDetail, true);

            var btnDone = new UIBarButtonItem (Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate {
                if( ViewModel.SaveCommand.CanExecute() )
                {
                    ViewModel.SaveCommand.Execute();
                }
            });

            NavigationItem.RightBarButtonItem = btnDone; 
            
        }

		
    }
}
