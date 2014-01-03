using System;
using Android.Runtime;
using Android.Widget;
using Android.Text;
using Android.Content;
using Android.Util;
using Java.Lang;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class UnderlineTextView : TextView
    {

        public UnderlineTextView(Context context)
            : base(context)
        {
        }
        
        public UnderlineTextView(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
        }
        
        public UnderlineTextView(IntPtr ptr, JniHandleOwnership handle)
            : base(ptr, handle)
        {
            
            
        }
        public override void SetText(ICharSequence text, BufferType type)
        {
            var s = string.Format( "<u>{0}</u>", text.ToString());
            base.SetText(Html.FromHtml( s ), type);
        }
    }
}

