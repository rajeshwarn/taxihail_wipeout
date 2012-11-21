
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
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
	public partial class LocationDetailView : MvxBindingTouchViewController<LocationDetailViewModel>
    {
        #region Constructors

        public event EventHandler Canceled;
        public event EventHandler Saved;
        public event EventHandler Deleted;

		public LocationDetailView () 
			: base(new MvxShowViewModelRequest<LocationDetailViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
		{
			Initialize();
		}
		
		public LocationDetailView (MvxShowViewModelRequest request) 
			: base(request)
		{
			Initialize();
		}
		
		public LocationDetailView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
			: base(request, nibName, bundle)
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
            
            ViewModel.Address.FullAddress = txtAddress.Text;
            Console.WriteLine("UpdateData" + txtAddress.Text);
			ViewModel.Address.Apartment = txtAptNumber.Text;
			ViewModel.Address.RingCode = txtRingCode.Text;
			ViewModel.Address.FriendlyName = txtName.Text;
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
                        
                        ViewModel.Address.Latitude = location.Latitude;
						ViewModel.Address.Longitude = location.Longitude;
                        
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
			ViewModel.RebookOrder.Execute();
        }

        public void LoadData(Address data)
        {
			ViewModel.Address = data;
            
            if (txtName != null)
            {
				if ((!ViewModel.Address.Id.IsNullOrEmpty()) || (ViewModel.Address.IsHistoric))
                {
					txtAddress.Text = ViewModel.Address.FullAddress;
					txtAptNumber.Text = ViewModel.Address.Apartment;
					txtRingCode.Text = ViewModel.Address.RingCode;
					txtName.Text = ViewModel.Address.FriendlyName;
                }


            }
        }
        #endregion
    }
}

