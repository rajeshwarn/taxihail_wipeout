using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using apcurium.MK.Booking.Api.Contract.Resources;
using apcurium.MK.Common.Extensions;
using apcurium.MK.Booking.Mobile.Extensions;
using apcurium.MK.Booking.Mobile.Client.InfoTableView;
using apcurium.MK.Booking.Mobile.ListViewStructure;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client
{
	public class LocationTableViewDelegate : UITableViewDelegate
	{

		private InfoStructure _structure;
		private LocationsTabView _parent;

		public LocationTableViewDelegate (LocationsTabView parent, InfoStructure structure)
		{
			_parent = parent;
			_structure = structure;
		}


		private Address _lastSelected;

		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return _structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ).RowHeight;
		}

		public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
		{

			if ( _structure.Sections.ElementAt( indexPath.Section ).EditMode )
			{
				var detail = new LocationDetailView ();
				_parent.NavigationController.PushViewController (detail, true);

				if (indexPath.Section == 0)
				{
					_lastSelected = (Address)_structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt (indexPath.Row).Data;					
				}

				else
				{
					_lastSelected = ((Address)_structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt (indexPath.Row).Data).Copy();					
                    _lastSelected.Id = Guid.Empty;
				}
				detail.LoadData (_lastSelected);

				detail.Deleted += HandleDetailDeleted;
				detail.Saved += HandleDetailSaved;
				
			}

			else // if( !_structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt( indexPath.Row ).Id.IsNullOrEmpty() )
			{
				_parent.DoSelect ( (Address)_structure.Sections.ElementAt( indexPath.Section ).Items.ElementAt (indexPath.Row).Data );
			}
			
		}

		public override UIView GetViewForHeader (UITableView tableView, int section)
		{
			var header = new UIView( new RectangleF( 0,0 ,320,33) );
			header.BackgroundColor = UIColor.Clear;

			var label = new UILabel( new RectangleF( 15, 5, 300, 25 ) );
			var color = _structure.Sections.ElementAt( section ).SectionLabelTextColor;
			label.TextColor = UIColor.FromRGBA( color[0], color[1], color[2], color[3] );
			label.Font = AppStyle.BoldTextFont;
			label.Text = _structure.Sections.ElementAt( section ).SectionLabel;
			label.BackgroundColor = UIColor.Clear;
			label.Text.PadLeft( 15 );

			header.AddSubview( label );

			return header;
		}

		void HandleDetailSaved (object sender, EventArgs e)
		{
            _parent.Update (_lastSelected);             
		}


		void HandleDetailDeleted (object sender, EventArgs e)
		{
			_parent.Delete (_lastSelected);
		}
		
	}
}


