using System;
using Cirrious.MvvmCross.Binding.Droid.Views;
using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using System.Drawing;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using apcurium.MK.Booking.Mobile.ViewModels.Orders;
using apcurium.MK.Common.Configuration;
using TinyIoC;
using System.Collections.ObjectModel;
using apcurium.MK.Booking.Mobile.ViewModels;
using System.Linq;
using System.Collections.Specialized;
using apcurium.MK.Booking.Mobile.Infrastructure;

namespace apcurium.MK.Booking.Mobile.Client.Controls.Widgets.Addresses
{
    public class AddressListView : MvxFrameControl
    {
        private bool isCollapsed;
        private bool HideViewAllButton;
        private LinearLayout _listLinearLayout;
        private Button _viewAllButton;

        public AddressListView(Context c, IAttributeSet attr) : base(c, attr)
        {
            HideViewAllButton = GetAttributeBool(attr, Resource.Attribute.HideViewAllButton);
        }

        private bool GetAttributeBool(IAttributeSet attr, int id)
        {
            var att = Context.ObtainStyledAttributes(attr, new int[] { id }, 0, 0);
            return att.GetBoolean(0, true);
        }

        protected override void OnFinishInflate()
        {
            base.OnFinishInflate();
            var inflater = (LayoutInflater) Context.GetSystemService(Context.LayoutInflaterService);
            var layout = inflater.Inflate(Resource.Layout.Control_AddressList, this, true);

            _listLinearLayout = (LinearLayout)layout.FindViewById<LinearLayout>(Resource.Id.ListLinearLayout);
            _viewAllButton = (Button)layout.FindViewById<Button>(Resource.Id.ViewAllButton);

            Addresses = new ObservableCollection<AddressViewModel>();
            AddressLines = new AddressLine[0];

            Addresses.CollectionChanged += (sender, e) =>
            {
                var newItems = new AddressViewModel[0];
                if (e.NewItems != null)
                {
                    newItems = e.NewItems.OfType<AddressViewModel>().ToArray();
                }

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    {                                    
                        AddressLines = AddressLines.Concat(newItems.Select(a => new AddressLine(Context, a, OnSelectAddress))).ToArray();
                        break;
                    }
                    case NotifyCollectionChangedAction.Reset:
                    {
                        AddressLines = new AddressLine[0];
                        break;
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException("Not supported " + e.Action);
                    }
                }   
            };

            Update();

            _viewAllButton.Click += (s, e) =>
            {
                if (!isCollapsed)
                {
                    Collapse();
                }
                else
                {
                    Expand();
                }
            };      
        }

        public ObservableCollection<AddressViewModel> Addresses { get; set; }
        public Action<AddressViewModel> OnSelectAddress { get; set; }

        private AddressLine[] _addresses;
        private AddressLine[] AddressLines
        {
            get { return _addresses;}
            set
            {
                _addresses = value;
                Update();
            }
        }

        private void Update()
        {
            if (AddressLines == null || !AddressLines.Any())
            {
                Visibility = ViewStates.Gone;
                return;
            }

            Visibility = ViewStates.Visible;

            Collapse();

            _viewAllButton.Visibility = (AddressLines.Count() <= 3) || HideViewAllButton ? ViewStates.Gone : ViewStates.Visible;
        }

        public void Expand()
        {
            _listLinearLayout.RemoveAllViews();
            // TODO: Use the good localize approach
            _viewAllButton.Text = "Collapse";

            var i = 0;
            foreach (var line in AddressLines)
            {
                _listLinearLayout.AddView(line);
                if (++i != AddressLines.Length || _viewAllButton.Visibility== ViewStates.Visible)
                {
                    _listLinearLayout.AddView(GetDivider());
                }
            }

            isCollapsed = false;
        }

        View GetDivider(){
            var v = new View(Context);
            v.LayoutParameters = new ViewGroup.LayoutParams(ViewGroup.LayoutParams.FillParent, 1);
            v.SetBackgroundColor(Android.Graphics.Color.Argb(255, 240, 240, 240));
            return v;
        }

        public void Collapse()
        {
            if(HideViewAllButton)
            {
                Expand();
                return;
            }

            _listLinearLayout.RemoveAllViews();
            // TODO: Use the good localize approach
            _viewAllButton.Text = "View All";

            var i = 0;
            foreach (var line in AddressLines.Take(3))
            {
                _listLinearLayout.AddView(line);
                if(++i != 3  || _viewAllButton.Visibility== ViewStates.Visible)
                {
                    _listLinearLayout.AddView(GetDivider());
                }
            }
            isCollapsed = true;
        }
    }
}