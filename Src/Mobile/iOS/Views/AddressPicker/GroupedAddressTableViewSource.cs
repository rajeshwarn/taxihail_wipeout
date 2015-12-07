using System;
using System.Linq;
using UIKit;
using Foundation;
using System.Collections.ObjectModel;
using CoreGraphics;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;
using apcurium.MK.Common.Entity;

namespace apcurium.MK.Booking.Mobile.Client.Views.AddressPicker
{
    public class GroupedAddressTableViewSource : BindableTableViewSource
    {
        private static UIImage PoweredBy = UIImage.FromFile("poweredBy4sq.png");
        private float NearZero = 0.000001f; // iOS doesn't accept 0 for the height

        private nint? _expandedSection;
        private int _collapseItemCount;
		private AddressLocationType _addressLocationTypePicker;

        public GroupedAddressTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
        base( tableView, cellStyle, identifier, bindingText, accessory)
        {
            _collapseItemCount = UIHelper.Is35InchDisplay ? 2 : 3;            
        }
			
		public AddressLocationType AddressLocationTypePicker
		{
			get
			{
				return _addressLocationTypePicker;
			}
			set
			{
				if (_addressLocationTypePicker != value) 
				{
					_addressLocationTypePicker = value;
					if(_addressLocationTypePicker == AddressLocationType.Airport || _addressLocationTypePicker == AddressLocationType.Train)
					{
						_collapseItemCount = int.MaxValue;
					}
					else
					{
						_collapseItemCount = UIHelper.Is35InchDisplay ? 2 : 3;
					}
				}
			}
		}

        public override nint NumberOfSections(UITableView tableView)
        {
            var collection = ItemsSource as ObservableCollection<AddressViewModel>;
            if (collection != null)
            {
                if (collection.Any(c => c.IsSearchResult))
                {
                    return 1;
                }
                else
                {
                    return collection.GroupBy(a => a.Type).Count();
                }
            }
            else
            {
                return 0;
            }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var collection = ItemsSource as ObservableCollection<AddressViewModel>;
            if (collection != null)
            {
                if (collection.Any(c => c.IsSearchResult))
                {
                    return collection.Count;
                }
                else if (_expandedSection == section)
                {
                    return collection.GroupBy(a => a.Type).ElementAt((int)section).Count();
                }
                else
                {
                    return collection.GroupBy(a => a.Type).ElementAt((int)section).Take(_collapseItemCount ).Count();
                }
            }
            else
            {
                return 0;
            }
        }

        protected override object GetItemAt(NSIndexPath indexPath)
        {
            var collection = ItemsSource as ObservableCollection<AddressViewModel>;
            if (collection != null)
            {
                if (collection.Any(c => c.IsSearchResult))
                {
                    var item = collection.ElementAt(indexPath.Row );
                    var isLast = indexPath.Row == (collection.Count - 1);
                    item.IsLast = isLast;
                    return item;
                }
                else
                {
                    //_collapseItemCount;
                    var items = collection.GroupBy(a => a.Type).ElementAt(indexPath.Section);
                    var item = items.ElementAt(indexPath.Row);
                    var hasMoreItems = items.Count() > _collapseItemCount;

                    var isLast = indexPath.Row == (items.Count() - 1);
                    item.IsLast = isLast && !hasMoreItems;
                    return item;
                }
            }
            else
            {
                return 0;
            }
        }

        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            if (section == 0)
            {
                return NearZero;
            }
            return 15;
        }

        public override nfloat GetHeightForFooter(UITableView tableView, nint section)
        {
            var collection = ItemsSource as ObservableCollection<AddressViewModel>;
            if (collection != null)
            {
                var items = collection.GroupBy(a => a.Type).ElementAt((int)section);
                var hasMoreItems = items.Count() > _collapseItemCount;
                if (hasMoreItems)
                {
                    return tableView.RowHeight;
                }
            }

            return NearZero;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 44;
        }

        public override UIView GetViewForFooter(UITableView tableView, nint section)
        {
            var showLoadMore = true;
            var isPlaces = false;

            var collection = ItemsSource as ObservableCollection<AddressViewModel>;
            if (collection != null)
            {
                var items = collection.GroupBy(a => a.Type).ElementAt((int)section);
                showLoadMore = items.Count() > _collapseItemCount;
                isPlaces = items.Any(i => i.Address.AddressType ==  "place");
            }

            var container = new UIView() { Frame = new CGRect(0, 0, tableView.Frame.Width, tableView.RowHeight) };
            container.BackgroundColor = UIColor.Clear;

            if (showLoadMore)
            {
                var button = new CommandButton
                { 
                    BackgroundColor = UIColor.White,
                    Frame = new CGRect(0, 0, container.Frame.Width, container.Frame.Height)
                };           
                var title = _expandedSection == section 
                            ? Localize.GetValue("Collapse") 
                            : Localize.GetValue("ViewAll");
                button.SetAttributedTitle(new NSMutableAttributedString(title, UIFont.FromName(FontName.HelveticaNeueLight, 38 / 2), UIColor.FromRGB(44, 44, 44)), UIControlState.Normal);
                button.TouchUpInside += (sender, e) =>
                {
                    if (_expandedSection == section)
                    {
                        _expandedSection = null;
                    }
                    else
                    {
                        _expandedSection = section;
                    }
                    tableView.ReloadData();

                    var newContentSize = new CGSize(tableView.ContentSize.Width, tableView.ContentSize.Height);
                    if(_expandedSection == section)
                    {
                        newContentSize.Height += 50; 
                    }
                    tableView.ContentSize = newContentSize;
                };

                container.AddSubview(button);

                if (isPlaces)
                {
                    var googleLogo = new UIImageView
                    {
						Frame = new CGRect(button.Frame.X, button.Frame.Bottom + 5, PoweredBy.Size.Width, PoweredBy.Size.Height),
                        Image = PoweredBy ,
                        AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin
                    };
                    container.AddSubview(googleLogo);
                    if(_expandedSection != section)
                    {
                        tableView.ContentSize = new CGSize(tableView.ContentSize.Width, tableView.ContentSize.Height + PoweredBy.Size.Height + 10);
                    }
                }
            }

            return container;
        }

        public Action<AddressViewModel> OnRowSelected { get; set; }

        public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
        {
            base.RowSelected (tableView, indexPath);

            if (OnRowSelected != null)
            {
                OnRowSelected(GetItemAt(indexPath) as AddressViewModel);
            }
        }
    }
}

