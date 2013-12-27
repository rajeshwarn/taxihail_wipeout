using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using apcurium.MK.Booking.Mobile.Client.ListViewStructure;
using apcurium.MK.Booking.Mobile.Framework.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.ListViewCell
{
	 class ListViewAdapter : BaseAdapter
	{
		private static SectionItemCellRegistry _itemHandlerRegistry = new SectionItemCellRegistry ();
		private Dictionary<int, InfoCell> _cells;
		
		static ListViewAdapter ()
		{
			_itemHandlerRegistry.Add<TextEditSectionItem,TextEditCell> (( item, tbl, context ) => new TextEditCell ((TextEditSectionItem)item, tbl, context));
			_itemHandlerRegistry.Add<SpinnerSectionItem,SpinnerCell> (( item, tbl, context ) => new SpinnerCell ((SpinnerSectionItem)item, tbl, context));
			//_itemHandlerRegistry.Add<BooleanSectionItem,BooleanCell> (( item, tbl, context ) => new BooleanCell ((BooleanSectionItem)item, tbl, context));
		}

		public static SectionItemCellRegistry ItemCellRegistry { get { return _itemHandlerRegistry; } }

		public ListViewAdapter (Context context, ListStructure structure ) : base()
		{
			Structure = structure;
			OwnerContext = context;
			_cells = new Dictionary<int, InfoCell>();
			
		}
		
		public ListStructure Structure {
			get;
			private set;
		}

		public Context OwnerContext {
			get;
			private set;
		}
		
		public override Java.Lang.Object GetItem (int position)
		{
			return position; 
		} 
		
		public override int Count {
			get {
				return Structure.ItemsCount;
			}
		}
		
		public override long GetItemId (int position)
		{
			return position;
		}
		
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var item = Structure.Sections.ElementAt ( 0 ).Items.ElementAt ( position );									

			var infoCell = _cells.GetValueOrDefault(position);			
			if( infoCell == null )
			{
				infoCell = _itemHandlerRegistry.Resolve ( item, parent, OwnerContext );
				_cells.Add( position, infoCell );
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

