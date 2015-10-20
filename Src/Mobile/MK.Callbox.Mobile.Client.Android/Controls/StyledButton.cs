using System;
using System.Linq;
using Android.Graphics.Drawables;
using Android.Widget;
using Android.Runtime;
using Android.Content;
using Android.Util;
using apcurium.MK.Booking.Mobile.Style;
using apcurium.MK.Callbox.Mobile.Client.Helpers;

namespace apcurium.MK.Callbox.Mobile.Client.Controls
{
	[Register("apcurium.mk.callbox.mobile.client.controls.StyledButton")]
    public sealed class StyledButton : Button
    {
        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public StyledButton(Context context)
            : base(context)
        {
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public StyledButton(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            var att = Context.ObtainStyledAttributes(attrs, new int[] { Resource.Attribute.ButtonStyle }, 0, 0);

            var style = att.GetText(0);


            if ( StyleManager.Current.ButtonFontSize.HasValue )
            {
                //TextSize = DrawHelper.GetPixelsFromPt(  StyleManager.Current.ButtonFontSize.Value * 0.3f );
            }

            ButtonStyle = StyleManager.Current.Buttons.Single( b=>b.Key.ToLower() == style.ToLower() );

            SetTextColor( ButtonStyle.TextColor.ConvertToColor() );
			SetTextShadow( ButtonStyle.TextShadowColor);
             
           // SetBackgroundDrawable ( new GradientDrawable(ButtonStyle.Colors.ConvertToIntArray() )); 

        }
        
        public StyledButton(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
            : base(ptr, handle)
        {
            
            
        }
        
        public ButtonStyle ButtonStyle
        {
            get;
            private set;
        }

		void SetTextShadow (ColorDefinition textShadowColor)
		{
			if(textShadowColor == null) return;

			this.SetShadowLayer(1,0,1,textShadowColor.ConvertToColor() );
		}


    }
}

