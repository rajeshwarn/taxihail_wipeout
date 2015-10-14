﻿(function () {
    var action = TaxiHail.Controller.action;

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            /* Favorite addresses*/
            "": "manageDefaultAddresses",   // #
            'addresses/default/add': 'addDefaultAddress',
            'addresses/default/edit/:id': 'editDefaultAddress',

            /* popular addresses*/
            "addresses/popular": "managePopularAddresses",
            "addresses/popular/add": "addPopularAddress",
            "addresses/popular/edit/:id": "editPopularAddress",

            /* Admin right*/
            "security": "manageSecurity",
            "accountsManagement": "accountsManagement",
            "accountManagement": "accountManagement",
            "accountManagement?accountID=:accountID": "accountManagement",

            /* test notifications */
            "sendpushnotification": "sendPushNotification",
            "sendtestemail": "sendTestEmail",
            
            /* terms and conditions */
            "termsandconditions": "termsAndConditions",

            /* notification settings */
            "notificationSettings": "manageNotificationSettings",

            /* paymentSettings */
            "paymentSettings": "managePaymentSettings",
            
            /* Tariffs */
            "tariffs": "manageTariffs", //#tariffs
            "tariffs/add/recurring": "addRecurringTariff", //#tariffs/add/recurring
            "tariffs/add/day": "addDayTariff", //#tariffs/add/day
            "tariffs/add/vehicle": "addVehicleTariff", //#tariffs/add/vehicle
            "tariffs/edit/:id": "editTariff", //#tariffs/edit/{GUID}

            /* Rules */
            "rules": "manageRules", //#rules
            "rules/add/disable/default": "addDefaultDisableRule", //#rules/add/recurring
            "rules/add/disable/recurring": "addRecurringDisableRule", //#rules/add/recurring
            "rules/add/disable/day": "addDayDisableRule", //#rules/add/day
            "rules/add/warning/default": "addDefaultWarningRule", //#rules/add/recurring
            "rules/add/warning/recurring": "addRecurringWarningRule", //#rules/add/recurring
            "rules/add/warning/day": "addDayWarningRule", //#rules/add/day
            "rules/edit/:id": "editRule", //#rules/edit/{GUID}

            /*Export*/
            "exportaccounts": "exportAccounts",
            "exportorders": "exportOrders",
            "exportpromotions": "exportPromotions",

            /*Signup*/
            "signup": "signup",
            

            /* Charge Accounts */
            "accounts": "manageAccounts",
            "accounts/add": "addAccount",
            "accounts/edit/:number": "editAccount",

            /* Ratings */
            "ratings": "manageRatings",
            "ratings/add": "addRating",
            "ratings/edit/:number": "editRating",

            /* Vehicle Types */
            "vehicleTypes": "manageVehicleTypes",
            "vehicleTypes/add": "addVehicleType",
            "vehicleTypes/edit/:id": "editVehicleType"
        },

        initialize: function (options) {
            $('.menu-zone').html(new TaxiHail.AdminMenuView().render().el);
            
            //default lat and long are defined in the default.aspx
            TaxiHail.geocoder.initialize(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude);
        },
        
        signup: function () {
            document.location.href = '../#signup';
        },

        manageNotificationSettings: function () {
            action(TaxiHail.NotificationSettingsController, 'index');
        },

        managePaymentSettings: function () {
            action(TaxiHail.PaymentSettingsController, 'index');
        },
        
        manageDefaultAddresses: function () {
            action(TaxiHail.DefaultAddressesController, 'index');
        },

        addDefaultAddress: function () {
            action(TaxiHail.DefaultAddressesController, 'add');
        },

        editDefaultAddress: function(id) {
            action(TaxiHail.DefaultAddressesController, 'edit', id);
        },
        
        managePopularAddresses : function () {
            action(TaxiHail.PopularAddressesController, 'index');
        },

        addPopularAddress : function () {
            action(TaxiHail.PopularAddressesController, 'add');
        },
        
        editPopularAddress : function (id) {
            action(TaxiHail.PopularAddressesController, 'edit', id);
        },

        manageSecurity: function () {
            action(TaxiHail.SecurityController, 'index');
        },
        
        accountsManagement: function () {
        	action(TaxiHail.AccountsManagementController, 'accountsManagement');
        },

        accountManagement: function ()
        {
        	action(TaxiHail.AccountsManagementController, 'accountManagement');
        },

        sendPushNotification: function ()
        {
            action(TaxiHail.NotificationController, 'sendpushnotification');
        },

        sendTestEmail: function () {
            action(TaxiHail.NotificationController, 'sendtestemail');
        },
        
        termsAndConditions: function() {
            action(TaxiHail.TermsAndConditionsController, 'index');
        },

        manageTariffs: function() {
            action(TaxiHail.TariffsController, 'index');
        },

        addRecurringTariff: function() {
            action(TaxiHail.TariffsController, 'addRecurring');
        },

        addDayTariff: function() {
            action(TaxiHail.TariffsController, 'addDay');
        },

        addVehicleTariff: function () {
            action(TaxiHail.TariffsController, 'addVehicle');
        },

        editTariff: function(id) {
            
            action(TaxiHail.TariffsController, 'edit', id);
        },

        manageRules: function () {
            action(TaxiHail.RulesController, 'index');
        },

        addDefaultDisableRule: function () {
            action(TaxiHail.RulesController, 'addDefaultDisable');
        },

        addRecurringDisableRule: function () {
            action(TaxiHail.RulesController, 'addRecurringDisable');
        },

        addDayDisableRule: function () {
            action(TaxiHail.RulesController, 'addDayDisable');
        },
        
        addDefaultWarningRule: function () {
            action(TaxiHail.RulesController, 'addDefaultWarning');
        },

        addRecurringWarningRule: function () {
            action(TaxiHail.RulesController, 'addRecurringWarning');
        },

        addDayWarningRule: function () {
            action(TaxiHail.RulesController, 'addDayWarning');
        },

        editRule: function (id) {

            action(TaxiHail.RulesController, 'edit', id);
        },

        exportAccounts: function () {
            action(TaxiHail.ExportController, 'exportAccounts');
        },

        exportOrders: function () {
            action(TaxiHail.ExportController, 'exportOrders');
        },

        exportPromotions:function(){
        	action(TaxiHail.ExportController, 'exportPromotions');
        },

        manageRatings: function () {
            action(TaxiHail.RideRatingsController, 'index');
        },

        addRating: function () {
            action(TaxiHail.RideRatingsController, 'add');
        },

        editRating: function (id) {
            action(TaxiHail.RideRatingsController, 'edit', id);
        },

        manageAccounts: function() {
            action(TaxiHail.AccountChargeController, 'index');
        },

        addAccount: function () {
            action(TaxiHail.AccountChargeController, 'add');
        },

        editAccount: function (id) {
            action(TaxiHail.AccountChargeController, 'edit', id);
        },

        manageVehicleTypes: function () {
            action(TaxiHail.VehicleTypeController, 'index');
        },

        addVehicleType: function () {
            action(TaxiHail.VehicleTypeController, 'add');
        },

        editVehicleType: function (id) {
            action(TaxiHail.VehicleTypeController, 'edit', id);
        }

    });

}());