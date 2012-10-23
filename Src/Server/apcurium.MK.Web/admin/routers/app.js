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
            "rates": "manageRates", //#rates
            "rates/add": "addRate" //#rates/new
        },

        initialize: function (options) {
            $('.menu-zone').html(new TaxiHail.AdminMenuView().render().el);
            
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
        
        grantAdminAccess : function () {
            renderView(TaxiHail.GrantAdminAccessView);
        },

        manageRates: function() {
            TaxiHail.Controller.action(TaxiHail.RatesController, 'index');
        },

        addRate: function() {
            TaxiHail.Controller.action(TaxiHail.RatesController, 'add');
        }
    });

}());