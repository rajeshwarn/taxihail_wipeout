using System;

namespace apcurium.MK.Booking.Mobile.Client.ListViewStructure
{
    public abstract class SectionItem
    {
// ReSharper disable once InconsistentNaming
        protected float? _rowHeight;

        public SectionItem(Section parent)
        {
            Parent = parent;
            Enabled = () => true;
            CanDelete = false;
        }

        public SectionItem()
        {
            Enabled = () => true;
            CanDelete = false;
        }

        public Section Parent { get; set; }

        public float RowHeight
        {
            get
            {
                if (_rowHeight.HasValue)
                {
                    return _rowHeight.Value;
                }
                return Parent.RowHeight;
            }
            set { _rowHeight = value; }
        }

        public string Label { get; set; }


        //public virtual string Identifier{get{return null;}}

        public Guid? Id { get; set; }

// ReSharper disable once MemberCanBeProtected.Global
// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public int? Key { get; set; }

        public object Data { get; set; }

        public bool Highlighted { get; set; }

        public TextAlignment LabelAlignment { get; set; }

        public TextWidth LabelWidth { get; set; }

// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool CanDelete { get; set; }

// ReSharper disable once UnusedAutoPropertyAccessor.Global
        public Func<bool> Enabled { get; set; }

        public bool Commit()
        {
            return true;
        }
    }

    public enum TextAlignment
    {
        AlignLeft,
        AlignCenter,
        AlignRight,
    }

    public enum TextWidth
    {
        Small,
        Medium,
        Large,
    }
}