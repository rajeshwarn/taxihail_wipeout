using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using TaxiMobile.ListViewStructure;

namespace TaxiMobile.ListViewCell
{
	public class SectionItemCellRegistry
	{
		private static Dictionary<Type,Func<SectionItem, ViewGroup, Context, InfoCell>> _itemHandlerRegistry = new Dictionary<Type, Func<SectionItem, ViewGroup, Context, InfoCell>>();
		
		public void Add<TItem,TCell>(Func<SectionItem, ViewGroup, Context, InfoCell> func) where TItem : SectionItem where TCell : InfoCell
		{
			_itemHandlerRegistry.Add ( typeof(TItem), func );
		}
		
		public InfoCell Resolve(SectionItem sectionItem, ViewGroup parent, Context context )
		{						
		
			var itemType = sectionItem.GetType ();						
			var func =  _itemHandlerRegistry[itemType];
			return func( sectionItem, parent, context ); 
			 
		}
		
		public string GetCellIdentifier( SectionItem sectionItem )
		{			
			return sectionItem.GetType ().Name;
		}
	}
}

