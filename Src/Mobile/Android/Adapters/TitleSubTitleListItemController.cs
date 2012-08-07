using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class TitleSubTitleListItemController
    {
        
        
        public TitleSubTitleListItemController(View view)
        {
            View = view;

          
            HasSubTitle = View.FindViewById<TextView>(Resource.Id.ListItemSubtitle) != null;

            if (HasSubTitle)
            {
                View.FindViewById<TextView>(Resource.Id.ListItemTitle).Typeface = AppFonts.Medium; ;
                View.FindViewById<TextView>(Resource.Id.ListItemSubtitle).Typeface = AppFonts.Regular;
            }
            else
            {

                View.FindViewById<TextView>(Resource.Id.ListItemTitle).Typeface = AppFonts.Regular;
            }
        }

        public View View { get; private set; }
        public bool HasSubTitle { get; private set; }


        public string Title
        {
            get { return View.FindViewById<TextView>(Resource.Id.ListItemTitle).Text; }
            set { View.FindViewById<TextView>(Resource.Id.ListItemTitle).Text = value; }
        }

        public string SubTitle
        {
            get
            {
                if (HasSubTitle)
                {
                    return View.FindViewById<TextView>(Resource.Id.ListItemSubtitle).Text;
                }
                else
                {
                    return "";
                }
            }
            set
            {
                if (HasSubTitle)
                {
                    View.FindViewById<TextView>(Resource.Id.ListItemSubtitle).Text = value;
                }
            }
        }


        public void SetBackImage(int resourceId)
        {
            View.FindViewById<ImageView>(Resource.Id.ListItemBackImage).SetScaleType(ImageView.ScaleType.FitXy);
            View.FindViewById<ImageView>(Resource.Id.ListItemBackImage).SetImageResource(resourceId);
        }

        public void SetNavIcon(int resourceId)
        {
            View.FindViewById<ImageView>(Resource.Id.ListItemNavIcon).SetImageResource(resourceId);
        }
    }
}