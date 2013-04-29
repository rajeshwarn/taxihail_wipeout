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
		ListItemAdapter _adapter;
		TextView _label;
	    private ImageView _imageLeftView;

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
			_adapter = new ListItemAdapter( Context, Resource.Layout.SpinnerTextWithImage , Resource.Id.labelSpinner, new List<ListItemData>());
			_adapter.SetDropDownViewResource( Resource.Layout.SpinnerTextWithImage );
		}

		protected override void OnFinishInflate ()
		{

			base.OnFinishInflate();
			var inflater = (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
			var layout = inflater.Inflate(Resource.Layout.SpinnerCell, this, true);

			_label = (TextView) layout.FindViewById( Resource.Id.label );
			_label.Focusable = false;
		    _imageLeftView = layout.FindViewById<ImageView>(Resource.Id.leftImage);
			if(_text != null) _label.Text = _text;
            if (_leftImage != null)
            {
                var resource = Resources.GetIdentifier(_leftImage.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    _imageLeftView.SetImageResource(resource);
                    _label.SetPadding(70.ToPixels(),0,0,0);
                }
            }
			var button = (Button)layout.FindViewById( Resource.Id.openSpinnerButton );
			button.Click += (object sender, EventArgs e) => {
				_spinner.PerformClick();
			};
			_spinner = (Spinner) layout.FindViewById( Resource.Id.spinner );
			_spinner.Adapter = _adapter;
			SelectItem();
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

		int _selectedKey = int.MinValue;
		public void SetSelection (int key)
		{
			_selectedKey = key;
			SelectItem();
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

        private string _leftImage;
        public string LeftImage
        {
            get
            {
                return _leftImage;
            }
            set
            {
                _leftImage = value;
                if (_imageLeftView != null)
                {
                    var resource = Resources.GetIdentifier (value.ToLower (), "drawable", Context.PackageName);
                    if (resource != 0)
                    {
                        _imageLeftView.SetImageResource(resource);
                        _label.OffsetLeftAndRight(70);
                    }
                };
            }
        }
		public IEnumerable<ListItem> Data {
			set {
				if(value != null)
				{
					var data = value.Select(i => i.Id == ListItem.NullId 
					                        ? ListItemData.NullListItemData 
					                        : new ListItemData { Key = i.Id, Value = i.Display, Image = i.Image }).ToList();
					_adapter.Clear();
					foreach (var item in data) {
						_adapter.Add(item);
					}
					SelectItem();
				}
			}
		}

		private void SelectItem()
		{
			var index = -1;
			for(int i= 0; i< _adapter.Count; i++)
			{
				var item = _adapter.GetItem(i);
				if(item.Key.Equals(_selectedKey))
				{
					index = i;
					break;
				}
			}
			if (index < 0)
			{
				return;
			}
			if(_spinner != null) _spinner.SetSelection(index);
		}
	}

	public class ListItemAdapter : ArrayAdapter <ListItemData>{

		public ListItemAdapter(Context context, int textViewResourceId) : base (context, textViewResourceId){

		}	

		
		public ListItemAdapter(Context context, int resource, List<ListItemData> items) : base(context, resource, items)
		{

		}

		public ListItemAdapter(Context context, int resource,int textViewResourceId, List<ListItemData> items) : base(context, resource,textViewResourceId, items)
		{

		}


		public override View GetDropDownView (int position, View convertView, ViewGroup parent)
		{
			//we use the same layout as the GetView so we call it, see adapter ctor call to change the layout
			return GetView (position, convertView, parent);
		}


		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var v = convertView;
			
			if (v == null) {
				var inflater = (LayoutInflater)Context.GetSystemService (Context.LayoutInflaterService);
				v = inflater.Inflate (Resource.Layout.SpinnerTextWithImage, null);
			}

			var p = (ListItemData)this.GetItem(position);
			if(p != null)
			{
				CheckedTextView list_title = (CheckedTextView)v.FindViewById (Resource.Id.labelSpinner);
				ImageView list_image = (ImageView)v.FindViewById (Resource.Id.imageSpinner);
			
				if (list_title != null) {
					list_title.Text = p.Value;
				}	
				if (list_image != null
				    && p.Image != null) {
					list_image.SetImageResource (int.Parse (p.Image));
				}
			}			
			return v;
		}
	}
}

