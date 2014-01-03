using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Android.App;
using Android.Views;
using Android.Widget;

using apcurium.MK.Booking.Mobile.ViewModels;
using Cirrious.MvvmCross.Binding.Android.Views;
using Java.Lang;

namespace apcurium.MK.Booking.Mobile.Client.Adapters
{
    public class GroupedLocationListAdapter : MvxBindableListAdapter
    {
        public static int TYPE_SECTION_HEADER = 0;
        private readonly Activity _context;

        public GroupedLocationListAdapter(Activity context)
            : base(context)
        {
            _context = context;
            Headers = new ArrayAdapter<string>(context, Resource.Layout.ListHeader);
            Sections = new List<LocationListAdapter>();
        }

        public IList<LocationListAdapter> Sections { get; set; }
        public ArrayAdapter<string> Headers { get; set; }

        public override int Count
        {
            get
            {
                int total = 0;
                var sections = ItemsSource as IEnumerable<SectionAddressViewModel>;
                if (sections != null)
                    foreach (var section in sections)
                    {
                        total += section.Addresses.Count() + 1;
                    }
                return total;
            }
        }

        public override int ViewTypeCount
        {
            get
            {
                int total = 1;
                var sections = ItemsSource as IEnumerable<SectionAddressViewModel>;
                if (sections != null)
                    total += sections.Count();

                return total;
            }
        }

        protected override void SetItemsSource(IList value)
        {
            Sections.Clear();
            Headers.Clear();
            var sections = value as IEnumerable<SectionAddressViewModel>;
            if (sections != null)
            {
                foreach (var section in sections)
                {
                    Sections.Add(new LocationListAdapter(_context, section.Addresses.ToList()));
                    Headers.Add(section.SectionTitle);
                }
            }
            base.SetItemsSource(value);
        }

        public override Object GetItem(int position)
        {
            foreach (var adapter in Sections)
            {
                int size = adapter.Count + 1;
                if (position == 0)
                {
                    return null;
                }
                if (position < size)
                {
                    return adapter.GetItem(position - 1);
                }
                position -= size;
            }

            return null;
        }

        public override int GetItemViewType(int position)
        {
            int type = 1;
            foreach (var adapter in Sections)
            {
                int size = adapter.Count + 1;
                if (position == 0)
                {
                    return TYPE_SECTION_HEADER;
                }
                if (position < size)
                {
                    return type + adapter.GetItemViewType(position - 1);
                }
                position -= size;
                type += adapter.ViewTypeCount;
            }
            return -1;
        }

        public override bool AreAllItemsEnabled()
        {
            return false;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            int sectionNum = 0;
            foreach (var adapter in Sections)
            {
                int size = adapter.Count + 1;

                // check if position inside this section
                if (position == 0)
                {
                    var header = Headers.GetView(sectionNum, convertView, parent);
                    if (header is TextView)
                    {
                        ((TextView) header).Typeface = AppFonts.Bold;
                    }
                    return header;
                }
                if (position < size)
                {
                    return adapter.GetView(position - 1, convertView, parent);
                }

                // otherwise jump into next section
                position -= size;
                sectionNum++;
            }
            return null;
        }
    }

    public static class ObjectTypeHelper
    {
        public static T Cast<T>(this Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }
    }
}