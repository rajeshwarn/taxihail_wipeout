using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Util;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Callbox.Mobile.Client.Helpers;

namespace apcurium.MK.Callbox.Mobile.Client.Controls
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
            HideLogo = att.GetBoolean(0, true);

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

            var logo = this.FindViewById<ImageView>(Resource.Id.MainLogo);
            if (HideLogo)
            {
                logo.Visibility = ViewStates.Invisible;
            }
            else
            {
                var d = Context.Resources.GetDrawable(Resource.Drawable.Logo) as BitmapDrawable;
                var w = d.Bitmap.Width;
                var h = d.Bitmap.Height;

                var layoutParams = new RelativeLayout.LayoutParams(DrawHelper.GetPixels((56 * w) / h), DrawHelper.GetPixels(56));
                layoutParams.AlignWithParent = true;
                logo.LayoutParameters = layoutParams;
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

            if (BackgroundSource.HasValue())
            {
                var id = Resources.GetIdentifier(BackgroundSource, "drawable", Context.PackageName);
                this.FindViewById<ImageView>(Resource.Id.BackgroundImage).SetImageResource(id);
            }

            if (StyleManager.Current.NavigationTitleColor != null)
            {
                var txt = FindViewById<TextView>(Resource.Id.ViewTitle);
                txt.Typeface = AppFonts.Medium;
				txt.SetTextSize(ComplexUnitType.Dip, 16);
                txt.SetTextColor(new Color(StyleManager.Current.NavigationTitleColor.Red, StyleManager.Current.NavigationTitleColor.Green, StyleManager.Current.NavigationTitleColor.Blue));
            }

            LoopAllChildren(this);


        }

        private void LoopAllChildren(View view)
        {
            if (view is ViewGroup)
            {

                var vGroup = view as ViewGroup;
                for (int i = 0; i < vGroup.ChildCount; i++)
                {
                    var child = vGroup.GetChildAt(i);
                    LoopAllChildren(child);


                }
            }
            else if ( view is TextView )
            {

                var tt = ((TextView)view).Typeface;

                if ( (tt != null) && (tt.IsBold) )
                {                    
                        ((TextView)view).Typeface = AppFonts.Medium;                                       
                }
                else if ((tt != null) && (tt.IsItalic))
                {
                    ((TextView)view).Typeface = AppFonts.Italic;                                       
                }
                else
                {
                    ((TextView)view).Typeface = AppFonts.Regular;
                }


                
            }
        }






    }
}