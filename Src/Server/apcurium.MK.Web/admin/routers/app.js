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
            "": "manageFavoritesDefault",   // #
            "grantadmin": "grantAdminAccess",
            "managepopularaddresses": "managePopularAddresses",
            "settings" : "manageCompanySettings",
            /* Rates */
            "rates": "manageRates", //#rates
            "rates/add/recurring": "addRecurringRate", //#rates/add/recurring
            "rates/add/day": "addDayRate", //#rates/add/day
            "rates/edit/:id": "editRate", //#rates/edit/{GUID}
            /* IBS exclusions */
            "exclusions": "manageIBSExclusions"
        },

        initialize: function (options) {
            $('.menu-zone').html(new TaxiHail.AdminMenuView().render().el);
            
        },

        
        manageCompanySettings : function () {
            var settings = new TaxiHail.CompanySettingsCollection(),
                view = this._tabView = new TaxiHail.ManageCompanySettingsView({
                    collection: settings
                });
            settings.fetch({
                url: '../api/settings',
                success: _.bind(function(collection, resp) {
                    renderView(view);
                }, this)
            });
        },
           
        manageFavoritesDefault: function () {
            var addresses = new TaxiHail.CompanyDefaultAddressCollection(),
                        view = this._tabView = new TaxiHail.ManageDefaultAddressesView({
                            collection: addresses
                        });

            var favorites = new TaxiHail.CompanyDefaultAddressCollection();
            favorites.fetch({
                url: '../api/admin/addresses',
                success: _.bind(function (collection, resp) {
                            addresses.reset(favorites.models);
                            renderView(view);
                        }, this)
            });
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

        manageRates: function() {
            TaxiHail.Controller.action(TaxiHail.RatesController, 'index');
        },

        addRecurringRate: function() {
            TaxiHail.Controller.action(TaxiHail.RatesController, 'addRecurring');
        },

        addDayRate: function() {
            TaxiHail.Controller.action(TaxiHail.RatesController, 'addDay');
        },

        editRate: function(id) {
            TaxiHail.Controller.action(TaxiHail.RatesController, 'edit', id);
        },

        manageIBSExclusions: function() {
            TaxiHail.Controller.action(TaxiHail.ExclusionsController, 'index');
        }
    });

}());