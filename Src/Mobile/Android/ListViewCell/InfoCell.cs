using System;
using Android.Content;
using Android.Views;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;

namespace apcurium.MK.Booking.Mobile.Client.ListViewCell
{
    public abstract class InfoCell
    {
        private View _cellView;

        public InfoCell(SectionItem item, ViewGroup parent, Context context)
        {
            Parent = parent;
            OwnerContext = context;
            Item = item;
        }


        public ViewGroup Parent { get; private set; }

        public Context OwnerContext { get; private set; }

        public SectionItem Item { get; private set; }

        public View CellView
        {
            get { return _cellView; }
            set
            {
                _cellView = value;
                //_cellView.Click -= CellSelected;
                //_cellView.Click += CellSelected;
            }
        }

        public virtual void Initialize()
        {
        }

        public virtual void CellSelect()
        {
        }

        public virtual void Dispose()
        {
            if (_cellView != null)
            {
                _cellView.Dispose();
                _cellView = null;
            }
        }
        
        public virtual void LoadData()
        {
        }
    }

    public abstract class InfoCell<T> : InfoCell where T : SectionItem
    {
        public InfoCell(T item, ViewGroup parent, Context context) : base(item, parent, context)
        {
        }

        public T SectionItem
        {
            get { return (T) base.Item; }
        }
    }
}