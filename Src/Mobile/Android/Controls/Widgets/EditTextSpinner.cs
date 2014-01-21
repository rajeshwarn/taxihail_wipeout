using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;

using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    public class EditTextSpinner : LinearLayout
    {
        private ListItemAdapter _adapter;
        private ImageView _imageLeftView;
        private TextView _label;
        private string _leftImage;
        private int _selectedKey = int.MinValue;
        private Spinner _spinner;
        private string _text;

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


        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                if (_label != null) _label.Text = value;
            }
        }

        public string LeftImage
        {
            get { return _leftImage; }
            set
            {
                _leftImage = value;
                if (_imageLeftView != null)
                {
                    var resource = Resources.GetIdentifier(value.ToLower(), "drawable", Context.PackageName);
                    if (resource != 0)
                    {
                        _imageLeftView.SetImageResource(resource);
                        _label.OffsetLeftAndRight(70);
                    }
                }
            }
        }

        public IEnumerable<ListItem> Data
        {
            set
            {
                if (value != null)
                {
                    var data = value.Select(i => i.Id.HasValue
                        ? new ListItemData {Key = i.Id.Value, Value = i.Display, Image = i.Image}
                        : ListItemData.NullListItemData).ToList();
                    _adapter.Clear();
                    foreach (var item in data)
                    {
                        _adapter.Add(item);
                    }
                    SelectItem();
                }
            }
        }

        public event EventHandler<AdapterView.ItemSelectedEventArgs> ItemSelected;

        private void Initialize()
        {
            _adapter = new ListItemAdapter(Context, Resource.Layout.SpinnerTextWithImage, Resource.Id.labelSpinner,
                new List<ListItemData>());
            _adapter.SetDropDownViewResource(Resource.Layout.SpinnerTextWithImage);
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.SpinnerCell, this, true);

            _label = (TextView) layout.FindViewById(Resource.Id.label);
            _label.Focusable = false;
            _imageLeftView = layout.FindViewById<ImageView>(Resource.Id.leftImage);
            if (_text != null) _label.Text = _text;
            if (_leftImage != null)
            {
                var resource = Resources.GetIdentifier(_leftImage.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    _imageLeftView.SetImageResource(resource);
                    _label.SetPadding(70.ToPixels(), 0, 0, 0);
                }
            }
            var button = (Button) layout.FindViewById(Resource.Id.openSpinnerButton);
            button.Click += (sender, e) => { _spinner.PerformClick(); };
            _spinner = (Spinner) layout.FindViewById(Resource.Id.spinner);
            _spinner.Adapter = _adapter;
            SelectItem();
            _spinner.ItemSelected -= HandleItemSelected;
            _spinner.ItemSelected += HandleItemSelected;

            _spinner.Prompt = Context.GetString(Resource.String.ListPromptSelectOne);
        }

        private void HandleItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (ItemSelected != null)
            {
                ItemSelected(this, e);
            }
        }

        public Spinner GetSpinner()
        {
            return _spinner;
        }

        public ArrayAdapter<ListItemData> GetAdapter()
        {
            return _adapter;
        }

        public void SetSelection(int key)
        {
            _selectedKey = key;
            SelectItem();
        }


        private void SelectItem()
        {
            var index = -1;
            for (int i = 0; i < _adapter.Count; i++)
            {
                var item = _adapter.GetItem(i);
                if (item.Key.Equals(_selectedKey))
                {
                    index = i;
                    break;
                }
            }
            if (index < 0)
            {
                return;
            }
            if (_spinner != null) _spinner.SetSelection(index);
        }
    }

    public class ListItemAdapter : ArrayAdapter<ListItemData>
    {
        public ListItemAdapter(Context context, int textViewResourceId) : base(context, textViewResourceId)
        {
        }


        public ListItemAdapter(Context context, int resource, List<ListItemData> items) : base(context, resource, items)
        {
        }

        public ListItemAdapter(Context context, int resource, int textViewResourceId, List<ListItemData> items)
            : base(context, resource, textViewResourceId, items)
        {
        }


        public override View GetDropDownView(int position, View convertView, ViewGroup parent)
        {
            //we use the same layout as the GetView so we call it, see adapter ctor call to change the layout
            return GetView(position, convertView, parent);
        }


        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var v = convertView;

            if (v == null)
            {
                var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
                v = inflater.Inflate(Resource.Layout.SpinnerTextWithImage, null);
            }

            var p = GetItem(position);
            if (p != null)
            {
                var listTitle = (CheckedTextView) v.FindViewById(Resource.Id.labelSpinner);
                var listImage = (ImageView) v.FindViewById(Resource.Id.imageSpinner);

                if (listTitle != null)
                {
                    listTitle.Text = p.Value;
                }
                if (listImage != null
                    && p.Image != null)
                {
                    listImage.SetImageResource(int.Parse(p.Image));
                }
            }
            return v;
        }
    }
}