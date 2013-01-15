using System;

namespace apcurium.MK.Common.Entity
{
    public class ListItem : ListItem<int>
    {
    }

    public class NullableListItem : NullableListItem<int>
    {
    }

    public class ListItem<TId> : NullableListItem<TId> where TId: struct
    {
        public new TId Id {
            get {
                return base.Id.GetValueOrDefault ();
            }
            set {
                base.Id = value;
            }
        }
    }

    public class NullableListItem<TId> where TId: struct
    {
        public Nullable<TId> Id { get; set; }
        public string Display { get; set; }
        public bool? IsDefault{ get; set; }
        public ListItem<TId> Parent { get; set; }
        public string Image { get; set; }
        
    }
}