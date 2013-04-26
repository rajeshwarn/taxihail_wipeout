using System;
using Android.Widget;
using Cirrious.MvvmCross.Interfaces.Commands;
using Android.Runtime;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Text.Method;
using Android.Graphics.Drawables;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class TipSlider: LinearLayout
	{			
		[Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
		public TipSlider(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{ 
			var inflater = (LayoutInflater)Context.GetSystemService (Context.LayoutInflaterService);
			inflater.Inflate (Resource.Layout.Control_TipSlider, this, true);
		}
		public override bool Enabled {
			get {
				return base.Enabled;
			}
			set {
				SeekBar.Enabled = value;
				base.Enabled = value;
			}
		}
		
		SeekBar _seekBar;
		SeekBar SeekBar 
		{
			get
			{
				if(_seekBar== null)
				{			

					
					_seekBar= FindViewById<SeekBar>(Resource.Id.seekBar);
				}

				return _seekBar;
			}
		}


		public int Percent {
			get
			{
				return SeekBar.Progress;
			}
			set{
				SeekBar.Progress = value;
			}
		}

		public event EventHandler PercentChanged;

		protected override void OnFinishInflate ()
		{
			base.OnFinishInflate ();

			SeekBar.ProgressChanged += (object sender, SeekBar.ProgressChangedEventArgs e) => {
				
				SeekBar.Progress = (int)(Math.Round(e.Progress/5.0)*5);
				if(PercentChanged !=null)
				{
					PercentChanged(this, new EventArgs());
				}
			};
		
		}
		



		/*
		public void Update (Context context, int width)
		{
			
			var layout= FindViewById<LinearLayout>(Resource.Id.tipSliderLayout);

			var xx= layout.Width;



			var x= new HeaderMarks(context,layout, width);

			var y = new SliderControl(context,layout, width);
		}
		*/
	}
}

