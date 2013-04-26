using System;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;
using Android.Runtime;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Text.Method;
using Android.Graphics.Drawables;
using Android.Graphics;
using Android.App;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class SliderControl : Control
	{
		public SliderControl (Context context, LinearLayout layout,int width) : base(context, layout,width, 60)
		{


			var grayBarControl = new ProgressBarControl(context,default(int),Resource.Drawable.sliderGrayBarBody,Resource.Drawable.sliderGrayBar, width);					
			var yellowBarControl = new ProgressBarControl(context,Resource.Drawable.sliderYellowBar,Resource.Drawable.sliderYellowBarBody,default(int), width);					
			yellowBarControl.Resize(60);
			AddViews(grayBarControl,yellowBarControl);

			/*

			var green= new View(Context);
			green.SetSize(50,60);
			green.SetBackgroundColor(Color.Green);

			var red= new View(Context);
			red.SetSize(50,60);
			red.AddLayoutRule(LayoutRules.AlignParentRight);
			red.SetBackgroundColor(Color.Red);

			var x= green.GetFrame();

			var pink= new View(Context);
			pink.SetFrame(green.GetFrame().Width,0,width-green.GetFrame().Width-red.GetFrame().Width,60);

			pink.SetBackgroundColor(Color.Pink);

			var y = pink.GetFrame();
			AddViews(red,green,pink);
			*/

		}


	}

}

