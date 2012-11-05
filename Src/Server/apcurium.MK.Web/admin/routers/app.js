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
            
            /* settings */
            "settings" : "manageCompanySettings",

            /* Tariffs */
            "tariffs": "manageTariffs", //#tariffs
            "tariffs/add/recurring": "addRecurringTariff", //#tariffs/add/recurring
            "tariffs/add/day": "addDayTariff", //#tariffs/add/day
            "tariffs/edit/:id": "editTariff", //#tariffs/edit/{GUID}
            /* IBS exclusions */
            "exclusions": "manageIBSExclusions"
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

        manageIBSExclusions: function() {
            action(TaxiHail.ExclusionsController, 'index');
        }
    });

}());