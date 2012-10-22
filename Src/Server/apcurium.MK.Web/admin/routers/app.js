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

        },
        mapView;

    TaxiHail.App = Backbone.Router.extend({
        routes: {
            "": "manageFavoritesDefault",   // #
            "grantadmin": "grantAdminAccess"
        },

        initialize: function (options) {
            $('.menuadmin').html(new TaxiHail.AdminMenuView().render().el);
            
        },

        admin: function() {


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
        }
    });

}());