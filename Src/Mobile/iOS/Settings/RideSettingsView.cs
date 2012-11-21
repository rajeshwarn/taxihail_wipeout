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
        public event EventHandler Closed;

        private BookingSettings _settings;
        private CustomRootElement _vehiculeTypeEntry;
        private int _selected = 0;
        private bool _autoSave;
        private bool _companyOnly;

        public RideSettingsView(MvxShowViewModelRequest request)
            : base(request, UITableViewStyle.Grouped, null, false)
        {
            
        }
        
        public RideSettingsView(MvxShowViewModelRequest request, UITableViewStyle style, Cirrious.MvvmCross.Dialog.Touch.Dialog.Elements.RootElement root, bool pushing)
            : base(request, style, root, pushing)
        {
            
        }

        public BookingSettings Result
        {
            get { return _settings; }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            var button = new MonoTouch.UIKit.UIBarButtonItem(Resources.DoneButton, UIBarButtonItemStyle.Plain, delegate
            {
                ViewModel.DoneCommand.Execute();
            });
            NavigationItem.HidesBackButton = true;
            NavigationItem.RightBarButtonItem = button;
            NavigationItem.Title = Resources.GetValue("View_RideSettings");

            
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
            LoadSettingsElements();
        }

        private void CloseView()
        {
            
            if (Closed != null)
            {
                Closed(this, EventArgs.Empty);
            }
            
            
            if (!_companyOnly)
            {
                this.NavigationController.PopViewControllerAnimated(true);
            }
        }

        private void ApplyChanges()
        {
            if (_autoSave)
            {
                ThreadHelper.ExecuteInThread(() =>
                {
                    TinyIoCContainer.Current.Resolve<IAccountService>().UpdateSettings(_settings);
                });
            }
            
        }

        private void LoadSettingsElementsForComaganies()
        {
            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    int index = 0;
                    int selected = 0;
                    
                    var companies = new Section(Resources.RideSettingsCompany);
                    var companiesList = TinyIoCContainer.Current.Resolve<IAccountService>().GetCompaniesList();
                    index = 0;
                    selected = 0;
                    foreach (ListItem company in companiesList)
                    {
                        if (!company.Display.ToSafeString().ToLower().Contains("test"))
                        {
                            var item = new RadioElementWithId(company.Display);
                            item.Id = company.Id;
                            item.Tapped += delegate
                            {
                                SetCompany(item); 
                            };
                            
                            companies.Add(item);
                            if (_settings.ProviderId == company.Id)
                            {
                                selected = index;
                            }
                            index++;
                        }
                    }
                    
                    var companyEntry = new RootElement(Resources.RideSettingsCompany, new RadioGroup(selected));
                    companyEntry.Add(companies);
                    
                    
                    
                    
                    this.InvokeOnMainThread(() => {
                        /*this.Root = companyEntry;*/ });
                    
                }
                finally
                {
                    
                }
                
            });
            
        }

        private void LoadSettingsElements()
        {
            
            
            ThreadHelper.ExecuteInThread(() =>
            {
                
                try
                {
                    
                    var vehicules = TinyIoCContainer.Current.Resolve<IAccountService>().GetVehiclesList();
                    var payements = TinyIoCContainer.Current.Resolve<IAccountService>().GetPaymentsList();

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
                        int index = 0;
                    
                    
                        foreach (ListItem vType in vehicules)
                        {
                            var item = new RadioElementWithId(vType.Display);
                            item.Id = vType.Id;
                            item.Tapped += delegate
                            {                                                       
                                SetVehiculeType(item);
                        
                                ((UINavigationController)this.ParentViewController).PopViewControllerAnimated(true);
                            };
                            vehiculeTypes.Add(item);
                            if (_settings.VehicleTypeId == vType.Id)
                            {
                                _selected = index;
                            }
                            index++;
                        }
                        _vehiculeTypeEntry = new CustomRootElement(Resources.RideSettingsVehiculeType, new RadioGroup(_selected));
                        _vehiculeTypeEntry.Add(vehiculeTypes);
                
                        var chargeTypes = new SectionWithBackground(Resources.RideSettingsChargeType);
                    
                    
                    
                        index = 0;
                        int selected = 0;
                    
                    
                        foreach (ListItem pay in payements)
                        {
                            var item = new RadioElementWithId(pay.Display);
                            item.Id = pay.Id;
                            item.Tapped += delegate
                            {
                                SetChargeType(item); 
                                ((UINavigationController)this.ParentViewController).PopViewControllerAnimated(true);
                            };
                            chargeTypes.Add(item);
                            if (_settings.ChargeTypeId == pay.Id)
                            {
                                selected = index;

                            }
                            index++;
                        }
                        var chargeTypeEntry = new CustomRootElement(Resources.RideSettingsChargeType, new RadioGroup(selected));
                        chargeTypeEntry.Add(chargeTypes);


                        menu.Add(settings);

//                  ******************************************************
//                  Update password menu item, remove comments to activate
//                  ******************************************************
//                  if( !_accountId.IsNullOrEmpty() )
//                  {
//                      _updatePasswordElement = new NavigationElement( Resources.View_UpdatePassword, () => {
//                          var args = new Dictionary<string, string>(){ {"accountId", _accountId.Value.ToString()} };
//                          var dispatch = TinyIoC.TinyIoCContainer.Current.Resolve<IMvxViewDispatcherProvider>().Dispatcher;
//                          dispatch.RequestNavigate(new MvxShowViewModelRequest(typeof(UpdatePasswordViewModel), args, false, MvxRequestedBy.UserAction));
//                      });
//                  
//                      settings.Add ( _updatePasswordElement );
//                  }

                        settings.Add(nameEntry);
                        settings.Add(phoneEntry);
                        settings.Add(passengerEntry);
                        settings.Add(_vehiculeTypeEntry);
                        settings.Add(chargeTypeEntry);
                
                    
                        /*this.Root = menu; */});
                    
                }
                finally
                {
                    
                }
                
            });
            
        }

        private void SetVehiculeType(RadioElementWithId item)
        {
            SetVehiculeType(item.Id, item.Caption);
        }

        private void SetVehiculeType(int id, string vehiculeName)
        {
            _settings.VehicleTypeId = id;           
            ApplyChanges();
            
        }

        private void SetChargeType(RadioElementWithId item)
        {
            _settings.ChargeTypeId = item.Id;   
            ApplyChanges();
            
        }

        private void SetCompany(RadioElementWithId item)
        {
            _settings.ProviderId = item.Id;
            ApplyChanges();
        }
    
    }
}

