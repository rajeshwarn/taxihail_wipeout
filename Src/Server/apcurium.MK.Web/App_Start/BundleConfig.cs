using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Optimization;
using apcurium.MK.Web.Optimization;

namespace apcurium.MK.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/assets").Include(
                "~/assets/js/persist-min.js",
                "~/assets/js/handlebars-1.0.rc.1.js",
                "~/assets/js/jquery.hotkeys.js",
                "~/assets/js/jquery.edit-in-place.js",
                "~/assets/js/jquery.validate.js",
                "~/assets/js/underscore.js",
                "~/assets/js/backbone.js",
                "~/assets/bootstrap/js/bootstrap-alert.js",
                "~/assets/bootstrap/js/bootstrap-transition.js",
                "~/assets/bootstrap/js/bootstrap-button.js",
                "~/assets/bootstrap/js/bootstrap-modal.js",
                "~/assets/bootstrap/js/bootstrap-dropdown.js",
                "~/assets/bootstrap/js/bootstrap-tooltip.js",
                "~/assets/js/bootstrap-tooltip.js",
                "~/assets/bootstrap/js/bootstrap-popover.js",
                "~/assets/bootstrap-datepicker/js/bootstrap-datepicker.js",
                "~/assets/bootstrap-timepicker/js/bootstrap-timepicker.js",
                "~/assets/js/spin.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                "~/common/utils.js",
                "~/common/mixins/ValidatedView.js",
                "~/common/models/Address.js",
                "~/common/models/UserAccount.js",
                "~/common/models/CompanyPopularAddress.js",
                "~/models/Order.js",
                "~/models/OrderStatus.js",
                "~/models/Settings.js",
                "~/models/NewAccount.js",
                "~/models/ReferenceData.js",
                "~/common/collections/AddressCollection.js",
                "~/common/collections/CompanyPopularAddressCollection.js",
                "~/collections/OrderCollection.js",
                "~/common/views/TemplatedView.js",
                "~/common/views/LoginStatusView.js",
                "~/common/views/AlertView.js",
                "~/views/BookView.js",
                "~/views/BookLaterView.js",
                "~/views/LoginView.js",
                "~/views/AddressControlView.js",
                "~/common/views/AddressSelectionView.js",
                "~/common/views/AddressListView.js",
                "~/views/FavoritesAndHistoryListView.js",
                "~/common/views/AddressItemView.js",
                "~/views/BookingConfirmationView.js",
                "~/views/BookingStatusView.js",
                "~/views/MapView.js",
                "~/views/SignupView.js",
                "~/views/UserAccountView.js",
                "~/views/ProfileView.js",
                "~/views/UpdatePasswordView.js",
                "~/views/ResetPasswordView.js",
                "~/views/OrderHistoryView.js",
                "~/views/OrderHistoryDetailView.js",
                "~/views/OrderItemView.js",
                "~/common/views/BootstrapConfirmationView.js",
                "~/views/FavoriteDetailsView.js",
                "~/common/views/AddFavoriteView.js",
                "~/views/FavoritesView.js",
                "~/services/OrderService.js",
                "~/services/DirectionInfoService.js",
                "~/common/services/AuthService.js",
                "~/common/services/GeocodingService.js",
                "~/common/services/GeolocationService.js",
                "~/common/services/PlacesService.js",
                "~/routers/App.js"));

            var templateBundle = new Bundle("~/bundles/templates")
                .IncludeDirectory("~/templates", "*.handlebars");

            templateBundle.Transforms.Add(new HandlebarsTransform());
            templateBundle.Transforms.Add(new JsMinify());
            bundles.Add(templateBundle);

            var resourcesBundle = new Bundle("~/bundles/resources")
                .IncludeDirectory("~/localization", "*.json");

            resourcesBundle.Transforms.Add(new ResourcesTransform());
            resourcesBundle.Transforms.Add(new JsMinify());
            bundles.Add(resourcesBundle);


            // Admin Bundles
            bundles.Add(new ScriptBundle("~/admin/bundles/assets").Include(
                "~/assets/js/persist-min.js",
                "~/assets/js/handlebars-1.0.rc.1.js",
                "~/assets/js/jquery.hotkeys.js",
                "~/assets/js/jquery.edit-in-place.js",
                "~/assets/js/jquery.validate.js",
                "~/assets/js/underscore.js",
                "~/assets/js/backbone.js",
                "~/assets/bootstrap/js/bootstrap-alert.js",
                "~/assets/bootstrap/js/bootstrap-transition.js",
                "~/assets/bootstrap/js/bootstrap-button.js",
                "~/assets/bootstrap/js/bootstrap-modal.js",
                "~/assets/bootstrap/js/bootstrap-dropdown.js",
                "~/assets/bootstrap/js/bootstrap-tooltip.js",
                "~/assets/js/bootstrap-tooltip.js",
                "~/assets/bootstrap/js/bootstrap-popover.js",
                "~/assets/bootstrap-datepicker/js/bootstrap-datepicker.js",
                "~/assets/bootstrap-timepicker/js/bootstrap-timepicker.js",
                "~/assets/js/spin.js"));

            bundles.Add(new ScriptBundle("~/admin/bundles/app").Include(
                "~/common/utils.js",
                "~/common/mixins/ValidatedView.js",
                "~/common/views/TemplatedView.js",
                "~/admin/controllers/Controller.js",
                "~/admin/controllers/RatesController.js",
                "~/admin/models/CompanyDefaultAddress.js",
                "~/admin/models/CompanyPopularAddress.js",
                "~/common/models/UserAccount.js",
                "~/admin/models/Rate.js",
                "~/common/models/CompanyPopularAddress.js",
                "~/admin/collections/CompanyDefaultAddressCollection.js",
                "~/admin/collections/RateCollection.js",
                "~/common/collections/CompanyPopularAddressCollection.js",
                "~/admin/views/AddPopularAddressView.js",
                "~/common/views/AlertView.js",
                "~/common/views/LoginStatusView.js",
                "~/common/views/AddressItemView.js",
                "~/common/views/AddressSelectionView.js",
                "~/common/views/AddressListView.js",
                "~/common/views/AddFavoriteView.js",
                "~/common/views/BootstrapConfirmationView.js",
                "~/admin/views/GrantAdminAccessView.js",
                "~/admin/views/AdminMenuView.js",
                "~/admin/views/ManageDefaultAddressesView.js",
                "~/admin/views/ManageRatesView.js",
                "~/admin/views/ManagePopularAddressesView.js",
                "~/admin/views/RateItemView.js",
                "~/admin/views/EditRateView.js",
                "~/common/services/AuthService.js",
                "~/common/services/GeocodingService.js",
                "~/common/services/GeolocationService.js",
                "~/common/services/PlacesService.js",
                "~/admin/routers/App.js"));

            var adminTemplateBundle = new Bundle("~/admin/bundles/templates")
                .IncludeDirectory("~/admin/templates", "*.handlebars");

            adminTemplateBundle.Transforms.Add(new HandlebarsTransform());
            adminTemplateBundle.Transforms.Add(new JsMinify());
            bundles.Add(adminTemplateBundle);

            var adminResourcesBundle = new Bundle("~/admin/bundles/resources")
                .IncludeDirectory("~/admin/localization", "*.json");

            adminResourcesBundle.Transforms.Add(new ResourcesTransform());
            adminResourcesBundle.Transforms.Add(new JsMinify());
            bundles.Add(adminResourcesBundle);

        }
    }
}