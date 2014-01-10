using System.Drawing;
using apcurium.MK.Booking.Mobile.Client.Controls;
using apcurium.MK.Booking.Mobile.Client.Localization;
using MonoTouch.UIKit;

namespace apcurium.MK.Booking.Mobile.Client.Views
{
    public sealed class BookViewActionsView : UIView
    {
        public BookViewActionsView ()
        {
            UserInteractionEnabled = true;

            RefreshCurrentLocationButton = AppButtons.CreateStandardButton( new RectangleF( 8,7,39,35 ) , "", AppStyle.ButtonColor.Black, "");
            ((GradientButton)RefreshCurrentLocationButton).SetImage ("Assets/gpsRefreshIcon.png");
            AddSubview ( RefreshCurrentLocationButton );

            ClearLocationButton = AppButtons.CreateStandardButton( new RectangleF( 8,7,39,35 ) , "", AppStyle.ButtonColor.Red, "Assets/cancel.png");
            AddSubview ( ClearLocationButton );


            BookNowButton = AppButtons.CreateStandardButton(new RectangleF(55, 7, 210, 35), Localize.GetValue("BookItButton"), AppStyle.ButtonColor.Green, "");
            AddSubview ( BookNowButton );

            BookLaterButton = AppButtons.CreateStandardButton( new RectangleF( 273,7,39,35 ) , "", AppStyle.ButtonColor.Black, "");
            AddSubview ( BookLaterButton );
            ((GradientButton)BookLaterButton).SetImage ("Assets/bookLaterIcon.png");  

        }

        public UIButton RefreshCurrentLocationButton {           
            get;
            private set;
        }

        public UIButton ClearLocationButton {
            get;
            private set;
        }

        public UIButton BookNowButton {           
            get;
            private set;
        }
        
        public UIButton BookLaterButton {
            get;
            private set;
        }


    }
}

