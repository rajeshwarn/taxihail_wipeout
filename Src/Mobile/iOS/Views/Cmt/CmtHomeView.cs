
using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Cirrious.MvvmCross.Binding.Touch.Views;
using Cirrious.MvvmCross.Views;
using apcurium.MK.Booking.Mobile.ViewModels;

namespace apcurium.MK.Booking.Mobile.Client.Views.Cmt
{
    public partial class CmtHomeView : MvxBindingTouchViewController<CmtHomeViewModel>
    {
        public CmtHomeView () 
            : base(new MvxShowViewModelRequest<CmtHomeViewModel>( null, true, new Cirrious.MvvmCross.Interfaces.ViewModels.MvxRequestedBy()   ) )
        {
            
        }
        
        public CmtHomeView (MvxShowViewModelRequest request) 
            : base(request)
        {
            
        }
        
        public CmtHomeView (MvxShowViewModelRequest request, string nibName, NSBundle bundle) 
            : base(request, nibName, bundle)
        {
            
        }
    }
}

