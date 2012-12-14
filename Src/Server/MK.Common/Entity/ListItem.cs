namespace apcurium.MK.Common.Entity
{
    public class ListItem
    {
        public int Id { get; set; }
        public string Display { get; set; }
        public bool? IsDefault{ get; set; }
        public ListItem Parent { get; set; }
        public string Image { get; set; }
    }
}