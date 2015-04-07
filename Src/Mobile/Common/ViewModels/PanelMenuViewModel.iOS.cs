using System;
using System.Collections.ObjectModel;
using System.Linq;
using apcurium.MK.Booking.Mobile.Extensions;


namespace apcurium.MK.Booking.Mobile.ViewModels
{
	// N.B.: This partial class file is for iOS only!
	public partial class PanelMenuViewModel
	{
		private const int PromotionItemMenuId = 4;

		private object _itemMenuListLock = new object();

		public ObservableCollection<ItemMenuModel> ItemMenuList { get; set; }

		partial void PartialConstructor()
		{
			ItemMenuList = new ObservableCollection<ItemMenuModel>();
		}

		private void InitDefaultIOSMenuList()
		{
			ItemMenuList.Clear();
			ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 0, Text = this.Services().Localize["PanelMenuViewLocationsText"], NavigationCommand = NavigateToMyLocations });
			ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 1, Text = this.Services().Localize["PanelMenuViewOrderHistoryText"], NavigationCommand = NavigateToOrderHistory });
			ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 2, Text = this.Services().Localize["PanelMenuViewUpdateProfileText"], NavigationCommand = NavigateToUpdateProfile });
			ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 9, Text = this.Services().Localize["PanelMenuViewAboutUsText"], NavigationCommand = NavigateToAboutUs });
			ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 11, Text = this.Services().Localize["PanelMenuViewSignOutText"], NavigationCommand = SignOut });
		}

		partial void InitIOSMenuList()
		{
			InitDefaultIOSMenuList();

			if (IsPayInTaxiEnabled)
			{
				ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 3, Text = this.Services().Localize["PanelMenuViewPaymentInfoText"], NavigationCommand = NavigateToPaymentInformation });
			}
			if (Settings.PromotionEnabled)
			{
				ItemMenuList.Add(new ItemMenuModel 
					{ 
						ItemMenuId = PromotionItemMenuId, 
						Text = this.Services().Localize["PanelMenuViewPromotionsText"], 
						NavigationCommand = NavigateToPromotions, 
						Alert = PromoCodeAlert.HasValue ? PromoCodeAlert.Value.ToString() : null
					});
			}
			if (IsNotificationsEnabled)
			{
				ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 5, Text = this.Services().Localize["PanelMenuViewNotificationsText"], NavigationCommand = NavigateToNotificationsSettings });
			}			
			if (IsTaxiHailNetworkEnabled)
			{
				ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 6, Text = this.Services().Localize["PanelMenuViewTaxiHailNetworkText"], NavigationCommand = NavigateToUserTaxiHailNetworkSettings });
			}
			if (Settings.TutorialEnabled)
			{
				ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 7, Text = this.Services().Localize["PanelMenuViewTutorialText"], NavigationCommand = NavigateToTutorial });
			}
			if (!Settings.HideCallDispatchButton)
			{
				ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 8, Text = this.Services().Localize["PanelMenuViewCallDispatchText"], NavigationCommand = Call });
			}
            if (DisplayReportProblem)
			{
				ItemMenuList.Add(new ItemMenuModel { ItemMenuId = 10, Text = this.Services().Localize["PanelMenuViewReportProblemText"], NavigationCommand = NavigateToReportProblem });
			}

			RefreshIOSMenu();
		}

		partial void RefreshIOSMenuBadges()
		{
			var itemMenu = ItemMenuList
				.Select((item, index) => new
				{
					ItemMenu = item,
					Index = index
				})
				.FirstOrDefault(item => item.ItemMenu.ItemMenuId == PromotionItemMenuId);

			if(itemMenu != null)
			{
				itemMenu.ItemMenu.Alert = PromoCodeAlert.HasValue
					? PromoCodeAlert.Value.ToString()
					: null;
				
				RefreshIOSMenu();
			}
		}

		partial void RefreshIOSMenu()
		{
			lock(_itemMenuListLock)
			{
				var orderedMenuItems = ItemMenuList.OrderBy(i => i.ItemMenuId).ToList();
				ItemMenuList.Clear();

				foreach (var item in orderedMenuItems)
				{
					ItemMenuList.Add(item);
				}
			}
		}
	}
}