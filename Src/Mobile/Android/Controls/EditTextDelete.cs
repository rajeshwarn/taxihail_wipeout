using System;
using System.Collections.Generic;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;


namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class EditTextDelete : EditText
    {
        protected EditTextDelete(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public EditTextDelete(Context context) : base(context)
        {
        }

        public EditTextDelete(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public EditTextDelete(Context context, IAttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
        {
        }


        protected override void OnDraw(Canvas canvas)
        {
            base.OnDraw(canvas);
            var deleteButton = new Button(Context);
            deleteButton.Click += (sender, args) => { Text = ""; };
            deleteButton.SetBackgroundDrawable(Resources.GetDrawable(Resource.Drawable.cancel));
            AddTouchables(new List<View> {deleteButton});
        }
    }
}