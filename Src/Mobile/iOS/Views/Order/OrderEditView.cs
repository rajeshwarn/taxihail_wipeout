using System;
using System.Drawing;
using Cirrious.MvvmCross.Binding.BindingContext;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TinyIoC;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using apcurium.MK.Booking.Mobile.Client.Controls.Binding;

namespace apcurium.MK.Booking.Mobile.Client.Views.Order
{
    public partial class OrderEditView : BaseBindableChildView<OrderEditViewModel>
    {
        private IAppSettings Settings;

        public OrderEditView (IntPtr handle) : base(handle)
        {
        }

        private void Initialize()
        {
            BackgroundColor = UIColor.Clear;

            // set labels

            // configure modal text fields
        }

        private void InitializeBinding()
        {
//            if (Settings.Data.ShowPassengerName)
//            {
//                lblName.Maybe(x => x.RemoveFromSuperview());
//                lblName.Maybe(x => x.RemoveFromSuperview());
//            }
//
//            if (Settings.Data.ShowPassengerNumber)
//            {
//                lblPassengers.Maybe(x => x.RemoveFromSuperview());
//                txtPassengers.Maybe(x => x.RemoveFromSuperview());
//            }
//
//            if (Settings.Data.ShowPassengerPhone)
//            {
//                lblPhone.Maybe(x => x.RemoveFromSuperview());
//                txtPhone.Maybe(x => x.RemoveFromSuperview());
//            }
        }
     
        public override void AwakeFromNib()
        {
            base.AwakeFromNib();

            Settings = TinyIoCContainer.Current.Resolve<IAppSettings>();
            var isThriev = Settings.Data.ApplicationName == "Thriev";

            UINib nib;
            if (isThriev)
            {
                nib = UINib.FromName ("OrderEditView_Thriev", null);
            } 
            else 
            {
                nib = UINib.FromName ("OrderEditView", null);
            }

            var view = (UIView)nib.Instantiate(this, null)[0];
            AddSubview(view);

            Initialize();

            this.DelayBind (() => {
                InitializeBinding();
            });
        }
    }
}

