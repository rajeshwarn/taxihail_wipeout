using System;
using System.Linq;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Entity;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.AppServices;
using Cirrious.MvvmCross.Interfaces.Views;
using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Interfaces.ViewModels;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Common.Extensions;
using System.Collections.Generic;
using Cirrious.MvvmCross.Dialog.Touch;
using Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class RideSettingsView : MvxTouchDialogViewController<RideSettingsViewModel>
    {
        public RideSettingsView(MvxShowViewModelRequest request)
            : base(request, UITableViewStyle.Grouped, null, false)
        {
            
        }
        
        public RideSettingsView(MvxShowViewModelRequest request, UITableViewStyle style, Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements.RootElement root, bool pushing)
            : base(request, style, root, pushing)
        {
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var button = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate
            {
                ViewModel.SaveCommand.Execute();
            });
            NavigationItem.HidesBackButton = true;
            NavigationItem.RightBarButtonItem = button;
            NavigationItem.Title = Resources.GetValue("View_RideSettings");

            LoadSettingsElements();
            
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationController.NavigationBar.Hidden = false;

            ((UINavigationController)ParentViewController).View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));
            
            View.BackgroundColor = UIColor.Clear; 
            TableView.BackgroundColor = UIColor.Clear;
            TableView.BackgroundView = new UIView { BackgroundColor = UIColor.Clear };
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
        }

        private void LoadSettingsElements()
        {
            this.InvokeOnMainThread(() => {

                var menu = new RootElement(this.Title);
                var settings = new Section(Resources.DefaultRideSettingsViewTitle);
            
                var nameEntry = new RightAlignedMvvmCrossEntryElement(Resources.RideSettingsName, "");
                nameEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.Default;
                nameEntry.Bind (this, "{'Value':{'Path':'Name','Mode':'TwoWay'}}");
                
            
                var phoneEntry = new RightAlignedMvvmCrossEntryElement(Resources.RideSettingsPhone, "");
                phoneEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.PhonePad;
                phoneEntry.Bind (this, "{'Value':{'Path':'Phone','Mode':'TwoWay'}}");

                var passengerEntry = new RightAlignedMvvmCrossEntryElement(Resources.RideSettingsPassengers, "");
                passengerEntry.KeyboardType = MonoTouch.UIKit.UIKeyboardType.NumberPad;
                passengerEntry.Bind (this, "{'Value':{'Path':'Passengers','Mode':'TwoWay'}}");
                
                var vehiculeTypes = new SectionWithBackground(Resources.RideSettingsVehiculeType);
            
                int selected = 0;
                foreach (ListItem vType in ViewModel.Vehicles)
                {
                    var item = new RadioElementWithId(vType.Id, vType.Display);
                    item.Tapped += ()=> {
                        ViewModel.SetVehiculeType.Execute(item.Id);
                        ((UINavigationController)this.ParentViewController).PopViewControllerAnimated(true);
                    };
                    vehiculeTypes.Add(item);
                    if (ViewModel.VehicleTypeId == vType.Id)
                    {
                        selected = Array.IndexOf(ViewModel.Vehicles, vType);
                    }
                }

                var vehiculeTypeEntry = new CustomRootElement(Resources.RideSettingsVehiculeType, new RadioGroup(selected));
                vehiculeTypeEntry.Add(vehiculeTypes);


                var chargeTypes = new SectionWithBackground(Resources.RideSettingsChargeType);

                selected = 0;
                foreach (ListItem pay in ViewModel.Payments)
                {
                    var item = new RadioElementWithId(pay.Id, pay.Display);
                    item.Tapped += () => {
                        ViewModel.SetChargeType.Execute(pay.Id);
                        ((UINavigationController)this.ParentViewController).PopViewControllerAnimated(true);
                    };
                    chargeTypes.Add(item);
                    if (ViewModel.ChargeTypeId == pay.Id)
                    {
                        selected = Array.IndexOf(ViewModel.Payments, pay);
                    }
                }
                var chargeTypeEntry = new CustomRootElement(Resources.RideSettingsChargeType, new RadioGroup(selected));
                chargeTypeEntry.Add(chargeTypes);

                menu.Add(settings);
                settings.Add(nameEntry);
                settings.Add(phoneEntry);
                settings.Add(passengerEntry);
                settings.Add(vehiculeTypeEntry);
                settings.Add(chargeTypeEntry);
            
                this.Root = menu;
            });
        }
    }
}

