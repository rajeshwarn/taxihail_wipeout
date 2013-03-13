(function () {
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
            "confirmemail": "confirmEmail",
            
            /* settings */
            "settings" : "manageCompanySettings",

            /* Tariffs */
            "tariffs": "manageTariffs", //#tariffs
            "tariffs/add/recurring": "addRecurringTariff", //#tariffs/add/recurring
            "tariffs/add/day": "addDayTariff", //#tariffs/add/day
            "tariffs/edit/:id": "editTariff", //#tariffs/edit/{GUID}

            /* Rules */
            "rules": "manageRules", //#rules
            "rules/add/default": "addDefaultRule", //#rules/add/recurring
            "rules/add/recurring": "addRecurringRule", //#rules/add/recurring
            "rules/add/day": "addDayRule", //#rules/add/day
            "rules/edit/:id": "editRule", //#rules/edit/{GUID}

            /* IBS exclusions */
            "exclusions": "manageIBSExclusions",

            /* Manage Booking Rules */
            "rules": "manageBookingRules",
            
            /*Export*/
            "exportaccounts": "exportaccounts",
            "exportorders": "exportorders"
        },

        initialize: function (options) {
            $('.menu-zone').html(new TaxiHail.AdminMenuView().render().el);
            
        },
        
        manageCompanySettings: function () {
            action(TaxiHail.CompanySettingsController, 'index');
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
        
        confirmEmail: function () {
            action(TaxiHail.SecurityController, 'confirmemail');
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

        editTariff: function(id) {
            
            action(TaxiHail.TariffsController, 'edit', id);
        },

        manageRules: function () {
            action(TaxiHail.RulesController, 'index');
        },

        addDefaultRule: function () {
            action(TaxiHail.RulesController, 'addDefault');
        },

        addRecurringRule: function () {
            action(TaxiHail.RulesController, 'addRecurring');
        },

        addDayRule: function () {
            action(TaxiHail.RulesController, 'addDay');
        },

        editRule: function (id) {

            action(TaxiHail.RulesController, 'edit', id);
        },

        manageIBSExclusions: function () {
            
            action(TaxiHail.ExclusionsController, 'index');
        },

        manageBookingRules: function () {
            
            action(TaxiHail.RulesController, 'index');
        },
        exportaccounts: function () {
           
        },
        exportorders: function () {
            
        }
    });

}());