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

            RefreshCurrentLocationButton = AppButtons.CreateStandardButton( new RectangleF( 8,7,39,35 ) , "", AppStyle.ButtonColor.Green, "");
            ((GradientButton)RefreshCurrentLocationButton).SetImage ("Assets/gpsRefreshIcon.png");
            AddSubview ( RefreshCurrentLocationButton );

            ClearLocationButton = AppButtons.CreateStandardButton( new RectangleF( 8,7,39,35 ) , "", AppStyle.ButtonColor.Red, "Assets/cancel.png");
            AddSubview ( ClearLocationButton );


            BookNowButton = AppButtons.CreateStandardButton( new RectangleF( 55,7,178,35 ) , Resources.BookItButton, AppStyle.ButtonColor.Green, "");
            ((GradientButton)BookNowButton).RoundedCorners = UIRectCorner.BottomLeft | UIRectCorner.TopLeft;
            AddSubview ( BookNowButton );

            BookLaterButton = AppButtons.CreateStandardButton( new RectangleF( 232,7,80,35 ) , "Later", AppStyle.ButtonColor.AlternateCorporateColor, "");
            ((GradientButton)BookLaterButton).RoundedCorners = UIRectCorner.BottomRight | UIRectCorner.TopRight;
            AddSubview ( BookLaterButton );




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

