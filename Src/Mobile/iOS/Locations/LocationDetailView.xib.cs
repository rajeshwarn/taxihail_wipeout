
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Booking.Mobile.AppServices;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Common.Diagnostic;
using TinyMessenger;
using apcurium.MK.Booking.Mobile.Messages;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client
{
    public partial class LocationDetailView : UIViewController
    {
        #region Constructors

        // The IntPtr and initWithCoder constructors are required for items that need 
        // to be able to be created from a xib rather than from managed code

        private Address _data;

        public event EventHandler Canceled;
        public event EventHandler Saved;
        public event EventHandler Deleted;

        public LocationDetailView(IntPtr handle) : base(handle)
        {
            Initialize();
        }

        [Export("initWithCoder:")]
        public LocationDetailView(NSCoder coder) : base(coder)
        {
            Initialize();
        }

        public LocationDetailView() : base("LocationDetailView", null)
        {
            Initialize();
        }

        void Initialize()
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Assets/background.png"));

            this.NavigationItem.HidesBackButton = false;
            this.NavigationItem.TitleView = new TitleView(null, Resources.GetValue("View_LocationDetail"), true);
            
            
            lblSaveAsAFavorite.Text = Resources.LocationDetailInstructionLabel;
            lblName.Text = Resources.LocationDetailGiveItANameLabel;

            ((TextField)txtAddress).PaddingLeft = 3;
            ((TextField)txtAptNumber).PaddingLeft = 3;
            ((TextField)txtRingCode).PaddingLeft = 3;
            ((TextField)txtName).PaddingLeft = 3;

            txtAddress.Placeholder = Resources.LocationDetailStreetAddressPlaceholder;
            txtAptNumber.Placeholder = Resources.LocationDetailAptPlaceholder;
            txtRingCode.Placeholder = Resources.LocationDetailRingCodePlaceholder;
            txtName.Placeholder = Resources.LocationDetailGiveItANamePlaceholder;

            txtAddress.ShouldReturn = delegate(UITextField textField)
            {
                return textField.ResignFirstResponder();
            };
            txtAptNumber.ShouldReturn = delegate(UITextField textField)
            {
                return textField.ResignFirstResponder();
            };
            txtRingCode.ShouldReturn = delegate(UITextField textField)
            {
                return textField.ResignFirstResponder();
            };
            txtName.ShouldReturn = delegate(UITextField textField)
            {
                return textField.ResignFirstResponder();
            };
            
            txtAddress.Ended += HandleTxtAddressEnded;


            AppButtons.FormatStandardButton((GradientButton)btnSave, Resources.SaveButton, AppStyle.ButtonColor.Green); 

            ((GradientButton)btnBook).SetTitle(Resources.BookItButton, UIControlState.Normal);
            AppButtons.FormatStandardButton((GradientButton)btnDelete, Resources.DeleteButton, AppStyle.ButtonColor.Red); 

            btnBook.TouchUpInside += BtnBookTouchUpInside;
            btnSave.TouchUpInside += BtnSaveTouchUpInside;
            btnDelete.TouchUpInside += BtnDeleteTouchUpInside;
            btnDelete.Hidden = false;

        }

        void HandleTxtAddressEnded(object sender, EventArgs e)
        {
            var address = txtAddress.Text;
            ThreadHelper.ExecuteInThread(() =>
            {
                
                var location = TinyIoCContainer.Current.Resolve<IGeolocService>().ValidateAddress(address);
                
                
                if ((location == null) || location.FullAddress.IsNullOrEmpty() || !location.HasValidCoordinate())
                {
                    return;
                }
                
                
                InvokeOnMainThread(() => {
                    txtAddress.Text = location.FullAddress; }
                );
            }
            );
            
        }

        private void UpdateData()
        {
            
            _data.FullAddress = txtAddress.Text;
            Console.WriteLine("UpdateData" + txtAddress.Text);
            _data.Apartment = txtAptNumber.Text;
            _data.RingCode = txtRingCode.Text;
            _data.FriendlyName = txtName.Text;
        }

        void BtnDeleteTouchUpInside(object sender, EventArgs e)
        {
            LoadingOverlay.StartAnimatingLoading(LoadingOverlayPosition.Center, null, 130, 30);
            View.UserInteractionEnabled = false;
            ThreadHelper.ExecuteInThread(() =>
            {
                
                try
                {
                    if (Deleted != null)
                    {
                        Deleted(this, EventArgs.Empty);
                    }
                    
                    InvokeOnMainThread(() => btnDelete.Hidden = true);
                    InvokeOnMainThread(() => this.NavigationController.PopViewControllerAnimated(true));
                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        LoadingOverlay.StopAnimatingLoading();
                        View.UserInteractionEnabled = true;
                    }
                    );
                }
            }
            );
            
            
        }

        void BtnSaveTouchUpInside(object sender, EventArgs e)
        {
            if (txtAddress.Text.IsNullOrEmpty())
            {
                MessageHelper.Show(Resources.InvalidAddressTitle, Resources.InvalidAddressMessage);
                return;
            }
            
            
            txtAddress.ResignFirstResponder();
            txtAptNumber.ResignFirstResponder();
            txtRingCode.ResignFirstResponder();
            txtName.ResignFirstResponder();
            
            LoadingOverlay.StartAnimatingLoading( LoadingOverlayPosition.Center, null, 130, 30);
            View.UserInteractionEnabled = false;

            var address = txtAddress.Text;

            ThreadHelper.ExecuteInThread(() =>
            {
                try
                {
                    var location = TinyIoCContainer.Current.Resolve<IGeolocService>().ValidateAddress(address);
                    
                    
                    if ((location == null) || location.FullAddress.IsNullOrEmpty() || !location.HasValidCoordinate())
                    {
                        
                        InvokeOnMainThread(() => MessageHelper.Show(Resources.InvalidAddressTitle, Resources.InvalidAddressMessage));
                        
                        return;
                        
                    }
                    
                    InvokeOnMainThread(() =>
                    {
                        txtAddress.Text = location.FullAddress;
                        
                        UpdateData();
                        
                        _data.Latitude = location.Latitude;
                        _data.Longitude = location.Longitude;
                        
                        if (Saved != null)
                        {                            
                            Saved(this, EventArgs.Empty);
                        }
                        btnDelete.Hidden = false;
                        this.NavigationController.PopViewControllerAnimated(true);
                    }
                    );
                    
                }
                catch (Exception ex)
                {
                    TinyIoCContainer.Current.Resolve<ILogger>().LogError(ex);
                }
                finally
                {
                    InvokeOnMainThread(() =>
                    {
                        LoadingOverlay.StopAnimatingLoading();
                        View.UserInteractionEnabled = true;
                    }
                    );
                }
            }
            );
            
        }
    
        void BtnBookTouchUpInside(object sender, EventArgs e)
        {
            var order = new Order();
            order.PickupAddress = _data;
            var account = TinyIoCContainer.Current.Resolve<IAccountService>().CurrentAccount;
            order.Settings = account.Settings;
            InvokeOnMainThread(() => NavigationController.PopToRootViewController(true));
            InvokeOnMainThread(() => TinyIoCContainer.Current.Resolve<ITinyMessengerHub>().Publish(new RebookRequested(this, order)));
        }

        public void LoadData(Address data)
        {
            _data = data;
            
            if (txtName != null)
            {
                if ((!_data.Id.IsNullOrEmpty()) || (_data.IsHistoric))
                {
                    txtAddress.Text = _data.FullAddress;
                    txtAptNumber.Text = _data.Apartment;
                    txtRingCode.Text = _data.RingCode;
                    txtName.Text = _data.FriendlyName;
                }
            }
			if(_data.Id.Equals(Guid.Empty))
			{
				btnDelete.Hidden = true;
				btnBook.Hidden = true;
			}
        }
        #endregion
    }
}

