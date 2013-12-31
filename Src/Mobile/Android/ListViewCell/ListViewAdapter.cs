using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using apcurium.MK.Booking.Mobile.Framework.Extensions;
using Java.Lang;

namespace apcurium.MK.Booking.Mobile.Client.ListViewCell
{
    internal class ListViewAdapter : BaseAdapter
    {
        private static readonly SectionItemCellRegistry ItemHandlerRegistry = new SectionItemCellRegistry();
        private readonly Dictionary<int, InfoCell> _cells;

        static ListViewAdapter()
        {
            ItemHandlerRegistry.Add<TextEditSectionItem, TextEditCell>(
                (item, tbl, context) => new TextEditCell((TextEditSectionItem) item, tbl, context));
            ItemHandlerRegistry.Add<SpinnerSectionItem, SpinnerCell>(
                (item, tbl, context) => new SpinnerCell((SpinnerSectionItem) item, tbl, context));
            //_itemHandlerRegistry.Add<BooleanSectionItem,BooleanCell> (( item, tbl, context ) => new BooleanCell ((BooleanSectionItem)item, tbl, context));
        }

        public ListViewAdapter(Context context, ListStructure structure)
        {
            Structure = structure;
            OwnerContext = context;
            _cells = new Dictionary<int, InfoCell>();
        }

        public static SectionItemCellRegistry ItemCellRegistry
        {
            get { return ItemHandlerRegistry; }
        }

        public ListStructure Structure { get; private set; }

        public Context OwnerContext { get; private set; }

        public override int Count
        {
            get { return Structure.ItemsCount; }
        }

        public override Object GetItem(int position)
        {
            return position;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = Structure.Sections.ElementAt(0).Items.ElementAt(position);

            var infoCell = _cells.GetValueOrDefault(position);
            if (infoCell == null)
            {
                infoCell = ItemHandlerRegistry.Resolve(item, parent, OwnerContext);
                _cells.Add(position, infoCell);
            }
            else
            {
                infoCell.Dispose();
            }

            infoCell.Initialize();
            infoCell.LoadData();

            return infoCell.CellView;
        }
    }
}