using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
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


        protected override void OnDraw(Android.Graphics.Canvas canvas)
        {
            base.OnDraw(canvas);
            var deleteButton = new Button(this.Context);
            deleteButton.Click += (sender, args) =>
                                      {
                                          this.Text = "";
                                      };
            deleteButton.SetBackgroundDrawable( Resources.GetDrawable(Resource.Drawable.cancel));
            this.AddTouchables(new List<View>(){deleteButton});
        }


    }
}