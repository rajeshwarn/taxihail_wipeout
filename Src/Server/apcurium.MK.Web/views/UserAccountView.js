(function () {

    TaxiHail.UserAccountView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=goToProfile]': 'goToProfile',
            'click [data-action=goToFavorites]': 'goToFavorites',
            'click [data-action=goToHistory]': 'goToHistory',
            'click [data-action=goToPassword]': 'goToPassword',
            'click .nav-tabs li>a': 'ontabclick'
        },

        initialize: function() {

        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            this.goToProfile();

            return this;
        },
        
        goToProfile: function (e) {
            if (e) {
                e.preventDefault();
            }
            this._profile = new TaxiHail.ProfileView({
                model: this.model
            });
            this._profile.render();
            this.$("#user-account-container").html(this._profile.el);
            
        },
        
        goToFavorites : function(e){
            if (e) {
                e.preventDefault();
            }
            
            var addresses = new TaxiHail.AddressCollection(),
                    view = new TaxiHail.FavoritesView({
                        collection: addresses
                    });

            var favorites = new TaxiHail.AddressCollection();
            var history = new TaxiHail.AddressCollection();
            favorites.fetch({
                url: 'api/account/addresses',
                success: _.bind(function (collection, resp) {
                    history.fetch({
                        url: 'api/account/addresses/history',
                        success: _.bind(function (collection, resp) {
                            addresses.reset(favorites.models.concat(history.models));
                            this.$("#user-account-container").html(view.el);
                        }, this)
                    });
                }, this)
            });
        },
        
        goToHistory : function (e) {
            if (e) {
                e.preventDefault();
            }
            var orders = new TaxiHail.OrderCollection();
            orders.fetch({
                url: 'api/account/orders',
                success: _.bind(function (model) {
                    this._history = new TaxiHail.OrderHistoryView({
                        collection:model
                    });
                    this._history.render();
                    this.$("#user-account-container").html(this._history.el);
                }, this)
                
            });
            
            
            
        },
        
        goToPassword : function (e) {
            if (e) {
                e.preventDefault();
            }
            this._password = new TaxiHail.UpdatePasswordView({
                model: this.model
            });
            this._password.render();
            this.$("#user-account-container").html(this._password.el);
        },
        
        selectTab: function ($tab) {
            $tab.addClass('active').siblings().removeClass('active');
        },
        
        ontabclick: function (e) {
            e.preventDefault();

            var $tab = $(e.currentTarget).parent('li');

            this.selectTab($tab);

        },
        
    });

}());
