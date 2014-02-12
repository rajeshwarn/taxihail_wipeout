using System;
using System.Linq;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Collections.ObjectModel;
using System.Drawing;
using apcurium.MK.Booking.Mobile.ViewModels;
using apcurium.MK.Booking.Mobile.Client.Extensions.Helpers;
using apcurium.MK.Booking.Mobile.Client.Controls.Widgets;
using apcurium.MK.Booking.Mobile.Client.Localization;

namespace apcurium.MK.Booking.Mobile.Client.Views.AddressPicker
{

    public class GroupedAddressTableViewSource : BindableCommandTableViewSource
    {
        private int? _expandedSection;
        private int _collapseItemCount;


        public GroupedAddressTableViewSource (UITableView tableView, UITableViewCellStyle cellStyle, NSString identifier, string bindingText, UITableViewCellAccessory accessory ) : 
            base( tableView, cellStyle, identifier, bindingText, accessory)
        {
            _collapseItemCount = UIScreen.MainScreen.Bounds.Height > 500 ? 3 : 2;            
        }

        public override int NumberOfSections(MonoTouch.UIKit.UITableView tableView)
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

        public override int RowsInSection(UITableView tableview, int section)
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
                    return collection.GroupBy(a => a.Type).ElementAt(section).Count();
                }
                else
                {
                    return collection.GroupBy(a => a.Type).ElementAt(section).Take(_collapseItemCount ).Count();
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

        public override float GetHeightForHeader(UITableView tableView, int section)
        {
            return 5;
        }

        public override float GetHeightForFooter(UITableView tableView, int section)
        {
            var collection = ItemsSource as ObservableCollection<AddressViewModel>;
            if (collection != null)
            {
                var items = collection.GroupBy(a => a.Type).ElementAt(section);
                var hasMoreItems = items.Count() > _collapseItemCount;
                if (hasMoreItems)
                {
                    return 55;
                }
            }

            return 0;

        }

        public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 45;
        }

        public override UIView GetViewForFooter(UITableView tableView, int section)
        {
            bool showLoadMore = true;
            bool isPlaces = false;
            var collection = ItemsSource as ObservableCollection<AddressViewModel>;
            if (collection != null)
            {
                var items = collection.GroupBy(a => a.Type).ElementAt(section);
                showLoadMore = items.Count() > _collapseItemCount;
                isPlaces = items.Any(i => i.Address.AddressType ==  "place");
            }

            var container = new UIView() { Frame = new RectangleF( 0,0, 320, 44 ) };
            container.BackgroundColor = UIColor.Clear;

            if (showLoadMore)
            {
                var frame = UIHelper.IsOS7orHigher ? new RectangleF(0, 0, 320, 44) : new RectangleF(9, 0, 302, 44);
                var button = new FlatButton() { Frame = frame};               

                string title = _expandedSection == section ? Localize.GetValue("CollapseButton") : Localize.GetValue("ViewAllButton");
                button.SetTitle(title, UIControlState.Normal);
                button.TouchUpInside += (sender, e) => {
                    if (_expandedSection == section)
                    {
                        _expandedSection = null;
                    }
                    else
                    {
                        _expandedSection = section;
                    }
                    tableView.ReloadData();
                };
                container.AddSubview(button);

                if (isPlaces)
                {
                    var googleLogo = new UIImageView(new RectangleF(button.Frame.X, button.Frame.Bottom + 1, 104, 16));
                    googleLogo.Image = UIImage.FromFile("poweredByGoogle.png");
                    googleLogo.AutoresizingMask = UIViewAutoresizing.FlexibleTopMargin;
                    container.AddSubview(googleLogo);
                }

            }
            return container;

        }

        public Action<AddressViewModel> OnRowSelected
        {
            get;
            set;
        }

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

