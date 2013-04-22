using System;
using Android.Content;
using Android.Widget;
using Android.Views;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class ProgressBarControl : Control
	{
		ImageView _barStart;
		ImageView _barEnd;
		ImageView _barBody;

		int _barEndWidth;
		int _barHeight;
		int _topLeft;

		const int ControlHeight = 60;

		public ProgressBarControl (Context c, int barStartImageId, int barBodyImageId, int barEndImageId, int width):base(c, width,ControlHeight)
		{
			_barStart = new ImageView(c);
			_barEnd = new ImageView(c);
			_barBody = new ImageView(c);	

			if(barStartImageId != default(int))
			{
				_barStart.SetImageResource(barStartImageId);
				_barStart.SetSize(_barStart.GetBitmapSize());

				AddViews(_barStart);
			}
							
			if(barEndImageId != default(int))
			{	
				_barEnd.SetImageResource(barEndImageId);				
				_barEnd.SetSize(_barEnd.GetBitmapSize());
				_barEnd.AddLayoutRule(LayoutRules.AlignParentRight);
				AddViews(_barEnd);
			}

			_barBody.SetBackgroundResource(barBodyImageId);

			var leftNubWidth = _barStart.GetSize().Width;

			var barWidth = width-leftNubWidth-_barEnd.GetSize().Width;
			var barHeight = _barBody.GetBackgroundBitmapSize().Height;

			_barBody.SetSize(barWidth, barHeight);
			_barBody.SetPosition(leftNubWidth,0);

			AddViews(_barBody);

			var barEndSize= _barEnd.GetSize();
			_barEndWidth = barEndSize.Width;			
			_topLeft= (ControlHeight-barEndSize.Height)/2;
			_barHeight = barEndSize.Height;
		}

		public void Resize(int endPosition)
		{
			_barEnd.SetWidth(endPosition);
			_barBody.SetFrame(_barStart.GetRight(), _barStart.GetTop(), _barEnd.GetLeft() - _barStart.GetRight(), _barHeight);

		}
	}
}

