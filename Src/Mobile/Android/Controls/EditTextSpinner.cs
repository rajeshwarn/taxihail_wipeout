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

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
	public class EditTextSpinner: LinearLayout
	{
		Spinner _spinner;
		ArrayAdapter<ListItemData> _adapter;
		TextView _label;

		public event EventHandler<AdapterView.ItemSelectedEventArgs> ItemSelected;

		[Register(".ctor", "(Landroid/content/Context;)V", "")]
		public EditTextSpinner(Context context)
			: base(context)
		{
			Initialize();
		}
		
		[Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
		public EditTextSpinner(Context context, IAttributeSet attrs)
			: base(context, attrs)
		{
			Initialize();
		}
		
		public EditTextSpinner(IntPtr ptr, Android.Runtime.JniHandleOwnership handle)
			: base(ptr, handle)
		{
			
			Initialize ();
		}

		void Initialize ()
		{
			_adapter = new ArrayAdapter<ListItemData>( Context, Resource.Layout.SpinnerText, new List<ListItemData>() );
			_adapter.SetDropDownViewResource( Android.Resource.Layout.SimpleSpinnerDropDownItem );
			
		}

		protected override void OnFinishInflate ()
		{
			base.OnFinishInflate();
			var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
			var layout = inflater.Inflate(Resource.Layout.SpinnerCell, this, true);

			_label = (TextView) layout.FindViewById( Resource.Id.label );
			if(_text != null) _label.Text = _text;
			var button = (Button)layout.FindViewById( Resource.Id.openSpinnerButton );
			button.Click += (object sender, EventArgs e) => {
				_spinner.PerformClick();
			};
			_spinner = (Spinner) layout.FindViewById( Resource.Id.spinner );
			_spinner.Adapter = _adapter;
			_spinner.SetSelection(_selectedIndex);
			_spinner.ItemSelected -= HandleItemSelected;
			_spinner.ItemSelected += HandleItemSelected;

			_spinner.Prompt = Context.GetString( Resource.String.ListPromptSelectOne );
		}

		void HandleItemSelected (object sender, AdapterView.ItemSelectedEventArgs e)
		{
			if (ItemSelected != null) {
				ItemSelected(this, e);
			}
		}

		public Spinner GetSpinner ()
		{
			return _spinner;
		}

		public ArrayAdapter<ListItemData> GetAdapter ()
		{
			return _adapter;
		}

		int _selectedIndex;
		public void SetSelection (int index)
		{
			_selectedIndex = index;
			if(_spinner != null) _spinner.SetSelection(index);
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
		public IEnumerable<ListItem> Data {
			set {
				if(value != null)
				{
					var data = value.Select(i => new ListItemData { Key = i.Id, Value = i.Display }).ToList();
					_adapter.Clear();
					foreach (var item in data) {
						_adapter.Add(item);
					}
				}
			}
		}


	}
}

