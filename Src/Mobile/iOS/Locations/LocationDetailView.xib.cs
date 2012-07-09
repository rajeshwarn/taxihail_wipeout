
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.Framework.Extensions;
using Microsoft.Practices.ServiceLocation;

namespace TaxiMobileApp
{
	public partial class LocationDetailView : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		private LocationData _data;

		private UIButton _btnGCancel;
		private UIButton _btnGDelete;
		private UIButton _btnGSave;


		public event EventHandler Canceled;
		public event EventHandler Saved;
		public event EventHandler Deleted;


		public LocationDetailView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public LocationDetailView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public LocationDetailView () : base("LocationDetailView", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			View.BackgroundColor = UIColor.FromPatternImage (UIImage.FromFile ("Assets/background.png"));
			
			var view = AppContext.Current.Controller.GetTitleView (null, Resources.LocationDetailViewTitle, true);
			
			this.NavigationItem.HidesBackButton = false;
			this.NavigationItem.TitleView = view;
			
			
			lblSaveAsAFavorite.Text = Resources.LocationDetailInstructionLabel;
			lblName.Text = Resources.LocationDetailGiveItANameLabel;
			
			txtAddress.Placeholder = Resources.LocationDetailStreetAddressPlaceholder;
			txtAptNumber.Placeholder = Resources.LocationDetailAptPlaceholder;
			txtRingCode.Placeholder = Resources.LocationDetailRingCodePlaceholder;
			txtName.Placeholder = Resources.LocationDetailGiveItANamePlaceholder;
			btnSave.SetTitle (Resources.SaveButton, UIControlState.Normal);
			btnDelete.SetTitle (Resources.DeleteButton, UIControlState.Normal);
			btnCancel.SetTitle (Resources.CancelBoutton, UIControlState.Normal);
			
			txtAddress.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			txtAptNumber.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			txtRingCode.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			txtName.ShouldReturn = delegate(UITextField textField) { return textField.ResignFirstResponder (); };
			
			txtAddress.Ended += HandleTxtAddressEnded;
			
			_btnGCancel = GlassButton.Wrap (btnCancel, AppStyle.LightButtonColor, AppStyle.LightButtonHighlightedColor);
			_btnGSave = GlassButton.Wrap (btnSave, AppStyle.AcceptButtonColor, AppStyle.AcceptButtonHighlightedColor);
			_btnGDelete = GlassButton.Wrap (btnDelete, AppStyle.CancelButtonColor, AppStyle.CancelButtonHighlightedColor);
			
			_btnGCancel.TouchUpInside += BtnCancelTouchUpInside;
			_btnGSave.TouchUpInside += BtnSaveTouchUpInside;
			_btnGDelete.TouchUpInside += BtnDeleteTouchUpInside;
			
//			btnCancel.TouchUpInside += BtnCancelTouchUpInside;
//			btnSave.TouchUpInside += BtnSaveTouchUpInside;
//			btnDelete.TouchUpInside += BtnDeleteTouchUpInside;
		}

		void HandleTxtAddressEnded (object sender, EventArgs e)
		{
			ThreadHelper.ExecuteInThread (() =>
			{
				
				var locations = ServiceLocator.Current.GetInstance<IBookingService> ().SearchAddress (txtAddress.Text);
				
				
				if (locations.Count () != 1 || locations[0].Address.IsNullOrEmpty () || !locations[0].Longitude.HasValue || !locations[0].Latitude.HasValue)
				{
					return;
				}
				
				
				InvokeOnMainThread (() => { txtAddress.Text = locations[0].Address; });
			});
			
		}

		private void UpdateData ()
		{
			
			_data.Address = txtAddress.Text;
			Console.WriteLine ("UpdateData" + txtAddress.Text);
			_data.Apartment = txtAptNumber.Text;
			_data.RingCode = txtRingCode.Text;
			_data.Name = txtName.Text;
		}

		void BtnDeleteTouchUpInside (object sender, EventArgs e)
		{
			LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
			View.UserInteractionEnabled = false;
			ThreadHelper.ExecuteInThread (() =>
			{
				
				try
				{
					if (Deleted != null)
					{
						Deleted (this, EventArgs.Empty);
					}
					
					_data.IsFromHistory = true;
					InvokeOnMainThread (() => _btnGDelete.Hidden = true);
					InvokeOnMainThread (() => this.NavigationController.PopViewControllerAnimated (true));
				}
				catch (Exception ex)
				{
					Console.WriteLine (ex.Message);
				}
				finally
				{
					InvokeOnMainThread (() =>
					{
						LoadingOverlay.StopAnimatingLoading (this.View);
						View.UserInteractionEnabled = true;
					});
				}
			});
			
			
		}

		void BtnSaveTouchUpInside (object sender, EventArgs e)
		{
			if (txtAddress.Text.IsNullOrEmpty ())
			{
				MessageHelper.Show (Resources.InvalidAddressTitle, Resources.InvalidAddressMessage);
				return;
			}
			
			
			txtAddress.ResignFirstResponder ();
			txtAptNumber.ResignFirstResponder ();
			txtRingCode.ResignFirstResponder ();
			txtName.ResignFirstResponder ();
			
			LoadingOverlay.StartAnimatingLoading (this.View, LoadingOverlayPosition.Center, null, 130, 30);
			View.UserInteractionEnabled = false;
			ThreadHelper.ExecuteInThread (() =>
			{
				try
				{
					var locations = ServiceLocator.Current.GetInstance<IBookingService> ().SearchAddress (txtAddress.Text);
					
					
					if (locations.Count () != 1 || locations[0].Address.IsNullOrEmpty () || !locations[0].Longitude.HasValue || !locations[0].Latitude.HasValue)
					{
						
						
						
						InvokeOnMainThread (() => MessageHelper.Show (Resources.InvalidAddressTitle, Resources.InvalidAddressMessage));
						
						return;
						
					}
					
					InvokeOnMainThread (() =>
					{
						txtAddress.Text = locations[0].Address;
						
						UpdateData ();
						
						_data.Latitude = locations[0].Latitude;
						_data.Longitude = locations[0].Longitude;
						
						if (Saved != null)
						{
							
							Saved (this, EventArgs.Empty);
						}
						_data.IsFromHistory = false;
						_btnGDelete.Hidden = false;
						this.NavigationController.PopViewControllerAnimated (true);
					});
					
				}
				catch (Exception ex)
				{
					Console.WriteLine (ex.Message);
				}
				finally
				{
					InvokeOnMainThread (() =>
					{
						LoadingOverlay.StopAnimatingLoading (this.View);
						View.UserInteractionEnabled = true;
					});
				}
			});
			
		}

		void BtnCancelTouchUpInside (object sender, EventArgs e)
		{
			if (Canceled != null)
			{
				Canceled (this, EventArgs.Empty);
			}
			
			this.NavigationController.PopViewControllerAnimated (true);
		}


		public void LoadData (LocationData data)
		{
			_data = data;
			
			if (txtName != null)
			{
				Console.WriteLine ("LoadData" + data.Name);
				txtAddress.Text = _data.Address;
				txtAptNumber.Text = _data.Apartment;
				txtRingCode.Text = _data.RingCode;
				txtName.Text = _data.Name;
				_btnGDelete.Hidden = _data.IsFromHistory;
				
				if (_data.IsFromHistory)
				{
					_btnGSave.Frame = new System.Drawing.RectangleF (_btnGSave.Frame.X + 40, _btnGSave.Frame.Y, _btnGSave.Frame.Width, _btnGSave.Frame.Height);
					_btnGCancel.Frame = new System.Drawing.RectangleF (_btnGCancel.Frame.X - 40, _btnGCancel.Frame.Y, _btnGCancel.Frame.Width, _btnGCancel.Frame.Height);
				}
				
				Console.Write (_data.IsFromHistory.ToString ());
			}
		}
		#endregion
	}
}

