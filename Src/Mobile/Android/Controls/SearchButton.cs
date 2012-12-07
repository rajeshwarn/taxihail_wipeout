using System;
using Android.Widget;
using Android.Runtime;
using Cirrious.MvvmCross.Interfaces.Commands;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Text.Method;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class SearchButton: LinearLayout
	{
		Button _button;
		public IMvxCommand NavigateCommand{ get; set; }
		
		[Register(".ctor", "(Landroid/content/Context;)V", "")]
		public SearchButton(Context context)
			: base(context)
		{
			Initialize();
		}
		
		[Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
		public SearchButton(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}
		
		public SearchButton(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
			: base(ptr, handle)
		{
			
			Initialize ();
		}
		
		void Initialize ()
		{
			
		}
		
		protected override void OnFinishInflate ()
		{
			base.OnFinishInflate();
			var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
			var layout = inflater.Inflate(Resource.Layout.Control_SearchButton, this, true);
			_button = (Button)layout.FindViewById( Resource.Id.navigateButton );
			
			if(_text != null) _button.Text = _text;
			if(_transformationMethod != null) _button.TransformationMethod = TransformationMethod;
			

			_button.Click += (object sender, EventArgs e) => {
				if(NavigateCommand != null) {
					NavigateCommand.Execute();
				}
			};
		}
		
		private string _text;
		public string Text {
			get {
				return _text;
			}set {
				_text = value;
				if(_button != null) _button.Text = value;
			}
		}
		
		private ITransformationMethod _transformationMethod;
		public ITransformationMethod TransformationMethod {
			get {
				return _transformationMethod;
			}
			set {
				_transformationMethod = value;
				if(_button != null) _button.TransformationMethod= value;
			}
		}
		
	}
}

