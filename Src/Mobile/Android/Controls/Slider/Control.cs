using System;
using Android.Widget;
using Android.Content;
using Android.Views;
using Android.Graphics;


namespace apcurium.MK.Booking.Mobile.Client
{
	public class Control : RelativeLayout
	{

		public Control (Context  context, LinearLayout layout, int width, int height) :base(context)
		{
			this.SetSize(width,height);

			layout.AddView(this);

		}

		public Control (Context  context, int width, int height) :base(context)
		{			
			this.SetSize(width,height);
		}

		public void AddViews (params View[] children)
		{
			foreach(var child in children)
			{
				var lp = child.LayoutParameters.AsRelative();

				AddView(child,lp);
			}		
		}



	
	}
}

