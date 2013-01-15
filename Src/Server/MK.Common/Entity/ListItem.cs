namespace apcurium.MK.Common.Entity
{
    public class ListItem : ListItem<int>
    {
		private static ListItem _nullListItem = new ListItem { Id = int.MinValue }; 
		public static ListItem NullListItem
		{
			get{
				return _nullListItem;
			}
		}
    }

    public class ListItem<TId> where TId: struct
    {
        public TId Id { get; set; }
        public string Display { get; set; }
        public bool? IsDefault{ get; set; }
        public ListItem<TId> Parent { get; set; }
        public string Image { get; set; }
    }
}