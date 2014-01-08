using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Optimization;
using apcurium.MK.Web.Optimization;

namespace apcurium.MK.Web
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles, string applicationKey)
        {
            bundles.Add(new ScriptBundle("~/bundles/assets").Include(
                "~/assets/js/localstorage-polyfill.js",
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
                "~/models/Order.js",
                "~/models/OrderStatus.js",
                "~/models/Settings.js",
                "~/models/CompanySettings.js",
                "~/models/NewAccount.js",
                "~/common/models/AvailableVehicle.js",
                "~/common/models/ReferenceData.js",
                "~/common/collections/AddressCollection.js",
                "~/common/collections/AvailableVehicleCollection.js",
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
                "~/views/GetTheAppView.js",
                "~/views/UserAccountView.js",
                "~/views/ProfileView.js",
                "~/views/UpdatePasswordView.js",
                "~/views/ResetPasswordView.js",
                "~/views/OrderHistoryView.js",
                "~/views/OrderHistoryDetailView.js",
                "~/views/OrderItemView.js",
                "~/common/views/BootstrapConfirmationView.js",
                "~/views/FavoriteDetailsView.js",
                "~/views/AddFavoriteView.js",
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

            var themeTemplatesDirectory = "~/themes/" + applicationKey + "/templates";
            if (Directory.Exists(HostingEnvironment.MapPath(themeTemplatesDirectory)))
            {
                templateBundle.IncludeDirectory(themeTemplatesDirectory, "*.handlebars");
            }

            templateBundle.Transforms.Add(new HandlebarsTransform());
            templateBundle.Transforms.Add(new JsMinify());
            bundles.Add(templateBundle);

            var resourcesBundle = new Bundle("~/bundles/resources")
                .IncludeDirectory("~/localization", "*.json");
            var themeLocalizationDirectory = "~/themes/" + applicationKey + "/localization";
            if (Directory.Exists(HostingEnvironment.MapPath(themeLocalizationDirectory)))
            {
                resourcesBundle.IncludeDirectory(themeLocalizationDirectory, "*.json");
            }

            resourcesBundle.Transforms.Add(new ResourcesTransform());
            resourcesBundle.Transforms.Add(new JsMinify());
            bundles.Add(resourcesBundle);


            // Admin Bundles
            bundles.Add(new ScriptBundle("~/admin/bundles/assets").Include(
                "~/assets/js/localstorage-polyfill.js",
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
                /* Models */
                "~/common/models/UserAccount.js",
                "~/common/models/ReferenceData.js",
                "~/common/models/Address.js",
                "~/admin/models/Tariff.js",
                "~/admin/models/Rule.js",
                "~/admin/models/CompanySettings.js",
                "~/admin/models/PaymentSettings.js",
                /* Collections */
                "~/common/collections/AddressCollection.js",
                "~/common/collections/AvailableVehicleCollection.js",
                "~/admin/collections/CompanySettingsCollection.js",
                "~/admin/collections/TariffCollection.js",
                "~/admin/collections/RuleCollection.js",
                /* Controllers */
                "~/admin/controllers/Controller.js",
                "~/admin/controllers/SecurityController.js",
                "~/admin/controllers/TariffsController.js",
                "~/admin/controllers/RulesController.js",
                "~/admin/controllers/ExclusionsController.js",
                "~/admin/controllers/CompanySettingsController.js",
                "~/admin/controllers/PaymentSettingsController.js",
                "~/admin/controllers/DefaultAddressesController.js",
                "~/admin/controllers/PopularAddressesController.js",
                "~/admin/controllers/ExportController.js",
                /* Views */
                "~/admin/views/AddPopularAddressView.js",
                "~/common/views/AlertView.js",
                "~/common/views/LoginStatusView.js",
                "~/common/views/AddressItemView.js",
                "~/common/views/AddressSelectionView.js",
                "~/common/views/AddressListView.js",
                "~/admin/views/AddFavoriteView.js",
                "~/common/views/BootstrapConfirmationView.js",
                "~/admin/views/GrantAdminAccessView.js",
                "~/admin/views/ConfirmEmailView.js",
                "~/admin/views/SendPushNotificationView.js",
                "~/admin/views/ExportAccountsView.js",
                "~/admin/views/ExportOrdersView.js",
                "~/admin/views/AdminMenuView.js",
                "~/admin/views/ManageDefaultAddressesView.js",
                "~/admin/views/ManageTariffsView.js",
                "~/admin/views/ManageRulesView.js",
                "~/admin/views/ManagePopularAddressesView.js",
                "~/admin/views/ManageCompanySettingsView.js",
                "~/admin/views/ManagePaymentSettingsView.js",
                "~/admin/views/SettingsItemView.js",
                "~/admin/views/RuleItemView.js",
                "~/admin/views/EditRuleView.js",
                "~/admin/views/TariffItemView.js",
                "~/admin/views/EditTariffView.js",
                "~/admin/views/ManageExclusionsView.js",
                /* Services */
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