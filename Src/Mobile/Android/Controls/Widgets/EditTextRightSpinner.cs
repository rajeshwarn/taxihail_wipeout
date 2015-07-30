using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.Helpers;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using apcurium.MK.Common.Entity;


namespace apcurium.MK.Booking.Mobile.Client.Controls
{
    [Register("apcurium.mk.booking.mobile.client.controls.EditTextRightSpinner")]
    public class EditTextRightSpinner : LinearLayout
    {
        private ListItemAdapter _adapter;
        private ImageView _imageLeftView;
        private ImageView _imageRightArrow;
        private TextView _label;
        private string _leftImage;
        private int _selectedKey = int.MinValue;
        private SpinnerOtherSupport _spinner;
        private string _text;
        bool _fromUI = false;
        bool _allowOther = false;

        ListItemData _otherItem;

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public EditTextRightSpinner(Context context)
            : base(context)
        {
            Initialize(null);
        }

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public EditTextRightSpinner(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            Initialize(attrs);
        }

        public override bool Enabled
        {
            get { return base.Enabled; }
            set
            {
                base.Enabled = value;
                this.SetBackgroundColor(value ? _initialBackgroundColor : Color.Transparent);
                if (_imageRightArrow != null)
                {
                    _imageRightArrow.Visibility = value 
                        ? ViewStates.Visible 
                        : ViewStates.Invisible;
                }
            }
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

        private IEnumerable<ListItem> _data;
        public IEnumerable<ListItem> Data
        {
            get { return _data; }
            set
            {
                if (value != null)
                {
                    _data = value;
                    var data = value.Select(i => i.Id.HasValue
                        ? new ListItemData {Key = i.Id.Value, Value = i.Display, Image = i.Image}
                        : ListItemData.NullListItemData).ToList();
                    _adapter.Clear();
                    foreach (var item in data)
                    {
                        _adapter.Add(item);
                    }

                    if (_allowOther)
                    {
                        _adapter.Add(_otherItem);
                    }
                    SelectItem();
                }
            }
        }

        public event EventHandler<AdapterView.ItemSelectedEventArgs> ItemSelected;

        public event EventHandler<String> OtherValueSelected;

        public event EventHandler SpinnerClicked;

        private Color _initialBackgroundColor;

        private void Initialize(IAttributeSet attrs)
        {
            _adapter = new ListItemAdapter(Context, Resource.Layout.SpinnerTextWithImage, Resource.Id.labelSpinner,
                new List<ListItemData>());
            _adapter.SetDropDownViewResource(Resource.Layout.SpinnerTextWithImage);

            if (attrs != null)
            {
                var att = Context.ObtainStyledAttributes(attrs, new int[] { Android.Resource.Attribute.Background }, 0, 0);
                _initialBackgroundColor = att.GetColor(0, -1);

                var attOther = Context.ObtainStyledAttributes(attrs, new int[] { Resource.Attribute.allowOtherSelection }, 0, 0);
                _allowOther = attOther.GetBoolean(0, false);
            }

            _otherItem = new ListItemData
                {
                    Key = Int32.MaxValue,
                    Value = this.Services().Localize["OtherListItemLabel"]
                };
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.SpinnerCellTextRight, this, true);

            _label = (TextView) layout.FindViewById(Resource.Id.label);
            _label.Focusable = false;
            _imageLeftView = layout.FindViewById<ImageView>(Resource.Id.leftImage);
            _imageRightArrow = layout.FindViewById<ImageView>(Resource.Id.rightArrow);
            if (_text != null) _label.Text = _text;
            if (_leftImage != null)
            {
                var resource = Resources.GetIdentifier(_leftImage.ToLower(), "drawable", Context.PackageName);
                if (resource != 0)
                {
                    _imageLeftView.SetImageResource(resource);
                    if (this.Services ().Localize.IsRightToLeft) {
                        _label.SetPadding (0, 0, 70.ToPixels (), 0);
                    } else {
                        _label.SetPadding (70.ToPixels (), 0, 0, 0);
                    }
                }
            }
            var button = (Button) layout.FindViewById(Resource.Id.openSpinnerButton);

            button.Click += (sender, e) =>
                {
                    if (Enabled)
                    {
                        _fromUI = true;
                        _spinner.PerformClick();

                        if (SpinnerClicked != null)
                        {
                            SpinnerClicked(this, e);
                        }
                    }
                };

            _spinner = (SpinnerOtherSupport) layout.FindViewById(Resource.Id.spinner);
            _spinner.Adapter = _adapter;
            SelectItem();

            _spinner.ItemSelected -= HandleItemSelected;
            _spinner.ItemSelected += HandleItemSelected;

            _spinner.Prompt = Context.GetString(Resource.String.ListPromptSelectOne);
        }



        private async void HandleItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {            
            var item = _adapter.GetItem(e.Position);
            if (item != _otherItem)
            {
                if (ItemSelected != null)
                {
                    ItemSelected.Invoke(this, e);
                }
            }
            else
            {
                //binding will send this event, so we need to handle it and not displaying the popup
				string value = null;
                if (_fromUI)
                {
                    value = await this.Services().Message.ShowPromptDialog(this.Services().Localize["OtherListItemLabel"], 
                        this.Services().Localize["OtherListItemPromptLabel"], () => { }, true);
					
                }
				if (OtherValueSelected != null)
				{
					OtherValueSelected(this, value);
				}
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
                //during initilization the binding will try to set the value, if there's none
                //we need to set the selected to other (i.e. different from 0)
                if (!_fromUI)
                {
                    index = _adapter.Count - 1;
                }
                else
                {

                    return;
                }
            }
            if (_spinner != null) _spinner.SetSelection(index);
        }
    }


    public class SpinnerOtherSupport : Spinner
    {
        [Register(".ctor", "(Landroid/content/Context;I)V", "")]
        public SpinnerOtherSupport(Context context, [GeneratedEnum] SpinnerMode mode) : base (context, mode)
        {}

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;II)V", "")]
        public SpinnerOtherSupport(Context context, IAttributeSet attrs, int defStyleAttr, [GeneratedEnum] SpinnerMode mode)
            : base (context, attrs, defStyleAttr, mode)
        {}

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;I)V", "")]
        public SpinnerOtherSupport(Context context, IAttributeSet attrs, int defStyleAttr)
            : base (context, attrs, defStyleAttr)
        {}

        protected SpinnerOtherSupport(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {}

        [Register(".ctor", "(Landroid/content/Context;)V", "")]
        public SpinnerOtherSupport(Context context) : base (context)
        {}

        [Register(".ctor", "(Landroid/content/Context;Landroid/util/AttributeSet;)V", "")]
        public SpinnerOtherSupport(Context context, IAttributeSet attrs) : base(context, attrs)
        {}


        public override void SetSelection(int position)
        {
            bool same = false;
            if (position == SelectedItemPosition)
            {
                same = true;
            }
            base.SetSelection(position);
            //the same value selected will not raised the event in the built-in control
            //to handle the other option we need to raise it even if already selected
            if (same
				&& OnItemSelectedListener!= null)
            {
                OnItemSelectedListener.OnItemSelected(this, SelectedView, position, SelectedItemId);
            }  
        }
    }
}