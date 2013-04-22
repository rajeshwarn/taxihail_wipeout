
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

namespace apcurium.MK.Booking.Mobile.Client
{
	public class HeaderMarks : Control
	{
		public HeaderMarks (Context c, LinearLayout layout, int width) :base(c, layout,width,60)
		{
			var x= new View(Context);
			x.SetSize(50,60);
			x.SetSize(50,60);
			x.SetBackgroundColor(Color.White);

		}
	}
}

