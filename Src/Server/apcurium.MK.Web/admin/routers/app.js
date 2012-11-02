(function () {
    var currentView,
        renderView = function(view, model) {
            // Call remove on current view
            // in case it was overriden with custom logic
            if(currentView && _.isFunction(currentView.remove)) {
                currentView.remove();
            }

            if(_.isFunction(view)) {
                currentView = new view({
                    model: model
                }).render();
            } else {
                currentView = view;
                view.model = model || view.model;
                view.render();
            }

            $('#main').html(currentView.el);

            return currentView;

        };

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            "": "manageDefaultAddresses",   // #
            'addresses/default/add': 'addDefaultAddress',
            'addresses/default/edit/:id': 'editDefaultAddress',
            "grantadmin": "grantAdminAccess",
            "managepopularaddresses": "managePopularAddresses",
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
            TaxiHail.Controller.action(TaxiHail.CompanySettingsController, 'index');
        },
           
        manageDefaultAddresses: function () {
            TaxiHail.Controller.action(TaxiHail.DefaultAddressesController, 'index');
        },

        addDefaultAddress: function () {
            TaxiHail.Controller.action(TaxiHail.DefaultAddressesController, 'add');
        },

        editDefaultAddress: function(id) {
            TaxiHail.Controller.action(TaxiHail.DefaultAddressesController, 'edit', id);
        },
        
        managePopularAddresses : function () {
            var addresses = new TaxiHail.CompanyPopularAddressCollection(),
                        view = this._tabView = new TaxiHail.ManagePopularAddressesView({
                            collection: addresses
                        });

            var popular = new TaxiHail.CompanyPopularAddressCollection();
            popular.fetch({
                url: '../api/admin/popularaddresses',
                success: _.bind(function (collection, resp) {
                    addresses.reset(popular.models);
                    renderView(view);
                }, this)
            });
        },
        
        grantAdminAccess : function () {
            renderView(TaxiHail.GrantAdminAccessView);
        },

        manageTariffs: function() {
            TaxiHail.Controller.action(TaxiHail.TariffsController, 'index');
        },

        addRecurringTariff: function() {
            TaxiHail.Controller.action(TaxiHail.TariffsController, 'addRecurring');
        },

        addDayTariff: function() {
            TaxiHail.Controller.action(TaxiHail.TariffsController, 'addDay');
        },

        editTariff: function(id) {
            TaxiHail.Controller.action(TaxiHail.TariffsController, 'edit', id);
        },

        manageIBSExclusions: function() {
            TaxiHail.Controller.action(TaxiHail.ExclusionsController, 'index');
        }
    });

}());