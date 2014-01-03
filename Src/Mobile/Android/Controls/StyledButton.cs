using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;

using apcurium.MK.Booking.Mobile.Style;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
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
            var att = Context.ObtainStyledAttributes(attrs, new[] {Resource.Attribute.ButtonStyle}, 0, 0);

            var style = att.GetText(0);


            if (StyleManager.Current.ButtonFontSize.HasValue)
            {
                SetTextSize(ComplexUnitType.Dip, StyleManager.Current.ButtonFontSize.Value);
            }

            ButtonStyle = StyleManager.Current.Buttons.Single(b => b.Key.ToLower() == style.ToLower());

            SetTextColor(ButtonStyle.TextColor.ConvertToColor());
            SetTextShadow(ButtonStyle.TextShadowColor);

            //SetBackgroundDrawable ( new GradientDrawable(ButtonStyle.Colors.ConvertToIntArray() )); 
        }


        public ButtonStyle ButtonStyle { get; private set; }

        private void SetTextShadow(ColorDefinition textShadowColor)
        {
            if (textShadowColor == null) return;

            SetShadowLayer(1, 0, 1, textShadowColor.ConvertToColor());
        }
    }
}