using System;
using Android.Views;
using Android.Util;
using Android.Content;
using Android.Widget;
using Android.Runtime;
using apcurium.MK.Common.Entity;
using System.Collections.Generic;
using System.Linq;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using System.ComponentModel;
using Cirrious.MvvmCross.Interfaces.Commands;
using Android.Text.Method;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class EditTextNavigate: LinearLayout
	{
		TextView _label;

		public IMvxCommand NavigateCommand{ get; set; }

		[Register(".ctor", "(Landroid/content/Context;)V", "")]
		public EditTextNavigate(Context context)
			: base(context)
		{
			Initialize();
		}
		
		[Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
		public EditTextNavigate(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}
		
		public EditTextNavigate(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
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
			var layout = inflater.Inflate(Resource.Layout.Control_EditTextNavigate, this, true);

			_label = (TextView) layout.FindViewById( Resource.Id.label );
			_label.Focusable = false;
			if(_text != null) _label.Text = _text;
			if(_transformationMethod != null) _label.TransformationMethod = TransformationMethod;

			var button = (Button)layout.FindViewById( Resource.Id.navigateButton );
			button.Click += (object sender, EventArgs e) => {
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
				if(_label != null) _label.Text = value;
			}
		}

		private ITransformationMethod _transformationMethod;
		public ITransformationMethod TransformationMethod {
			get {
				return _transformationMethod;
			}
			set {
				_transformationMethod = value;
				if(_label != null) _label.TransformationMethod= value;
			}
		}

	}
}

