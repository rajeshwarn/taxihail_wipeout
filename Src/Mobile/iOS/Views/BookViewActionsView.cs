using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace apcurium.MK.Booking.Mobile.Client
{
    public class BookViewActionsView : UIView
    {
        public BookViewActionsView ()
        {

            UserInteractionEnabled = true;

            RefreshCurrentLocationButton = AppButtons.CreateStandardButton( new RectangleF( 8,7,39,35 ) , "", AppStyle.ButtonColor.Blue, "");
            ((GradientButton)RefreshCurrentLocationButton).SetImage ("Assets/gpsRefreshIcon.png");
            AddSubview ( RefreshCurrentLocationButton );

            ClearLocationButton = AppButtons.CreateStandardButton( new RectangleF( 8,7,39,35 ) , "", AppStyle.ButtonColor.Red, "Assets/cancel.png");
            AddSubview ( ClearLocationButton );


            BookNowButton = AppButtons.CreateStandardButton( new RectangleF( 114,7,93,35 ) , Resources.BookItButton, AppStyle.ButtonColor.Green, "");
            AddSubview ( BookNowButton );

            BookLaterButton = AppButtons.CreateStandardButton( new RectangleF( 273,7,39,35 ) , "", AppStyle.ButtonColor.DarkGray, "");
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

