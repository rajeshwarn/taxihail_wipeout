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
using Android.Util;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class HeaderedLayout : LinearLayout
    {

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public HeaderedLayout(Context context)
            : base(context)
        {
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public HeaderedLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {

            var att = Context.ObtainStyledAttributes(attrs, new int[] { Resource.Attribute.HideLogo }, 0, 0);
            HideLogo = att.GetBoolean(0, false);

            att = Context.ObtainStyledAttributes(attrs, new int[] { Resource.Attribute.RightButtonSource }, 0, 0);
            RightButtonSource = att.GetText(0);

            att = Context.ObtainStyledAttributes(attrs, new int[] { Resource.Attribute.BackgroundSource }, 0, 0);
            BackgroundSource = att.GetText(0);

        }

        public HeaderedLayout(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {


        }

        public bool HideLogo { get; set; }
        public string RightButtonSource { get; set; }
        public string BackgroundSource { get; set; }
        

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var childViews = new List<View>();
            for (int i = 0; i < this.ChildCount; i++)
            {
                var child = GetChildAt(i);
                childViews.Add(child);
                RemoveView(child);
            }

            var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_HeaderedLayout, this, true);
            var contentLayout = layout.FindViewById<LinearLayout>(Resource.Id.ContentLayout);

            foreach (var child in childViews)
            {
                contentLayout.AddView(child);
            }

            if (HideLogo)
            {
                this.FindViewById<ImageView>(Resource.Id.MainLogo).Visibility = ViewStates.Invisible;
            }

            if (string.IsNullOrEmpty(RightButtonSource))
            {
                this.FindViewById<ImageView>(Resource.Id.ViewNavBarRightButton).Visibility = ViewStates.Invisible;
            }
            else
            {
                var id = Resources.GetIdentifier(RightButtonSource, "drawable", Context.PackageName);
                this.FindViewById<ImageView>(Resource.Id.ViewNavBarRightButton).SetImageResource(id);
            }

            if ( BackgroundSource.HasValue() )
            {
                var id = Resources.GetIdentifier(BackgroundSource, "drawable", Context.PackageName);
                this.FindViewById<ImageView>(Resource.Id.BackgroundImage).SetImageResource(id);
            }

        }





    }
}