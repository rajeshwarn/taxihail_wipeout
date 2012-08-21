using System;
using System.Linq;
using System.Collections.Generic;
using apcurium.MK.Common.Extensions;

namespace apcurium.MK.Booking.Mobile.Client.InfoTableView
{
	public class InfoStructure : IDisposable
	{
		private List<Section> _sections;
		
		public event EventHandler ItemDeleted;
		
		public InfoStructure ( float defaultRowHeight, bool useSectionIndex )
		{
			UseSectionIndex = useSectionIndex;
			_sections = new List<Section>();
			RowHeight = defaultRowHeight;
		}
		public void DoItemDeleted( SectionItem item )
		{
			if ( ItemDeleted!=null )
			{				
				ItemDeleted( this, EventArgs.Empty );
			}
		}
		
		
		public bool UseSectionIndex{
			get;
			set;
		}
		
		public IEnumerable<Section> Sections {
			get { return _sections; }
		}
		
		
		public Section AddSection ( )
		{
			var section = new  Section( this );
			_sections.Add ( section );
			return section;
		}		

		public Section AddSection ( string sectionLabel )
		{
			var section = AddSection();
			section.SectionLabel = sectionLabel;
			return section;
		}
		
		public float RowHeight {
			get;
			set;
		}
		
		public IEnumerable<SectionItem> AllItems
		{
			get{ return Sections.SelectMany( section => section.Items );}
		}
		
		
		public int ItemsCount {
			get { return Sections.Sum ( s => s.Items.Count() ); }
		}

		public int SectionsCount {
			get { return Sections.Count(); }
		}
		
		public int TableViewHeight {
			get { return Sections.Sum ( s => s.Items.Sum ( i => (int)i.RowHeight ) ); }
		}		
		
		#region IDisposable implementation
		public void Dispose ()
		{
			
			Sections.ForEach ( s=> s.Dispose () );
			_sections = null;
		}

		public void Clear ()
		{
			Sections.Maybe (()=> Sections.ForEach ( s=> s.Clear () ));
			
			_sections = new List<Section>();
			
		}
		#endregion
	}
}

