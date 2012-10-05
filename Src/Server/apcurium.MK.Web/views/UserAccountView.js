(function () {

    TaxiHail.UserAccountView = TaxiHail.TemplatedView.extend({

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            return this;
        },
        
        tab: {
            profile: function() {
                this._profile = new TaxiHail.ProfileView({
                    model: this.model
                    });
                this._profile.render();
                this.$("#user-account-container").html(this._profile.el);
            },

            favorites: function(){
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
            history: function () {
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
            password: function () {
                this._password = new TaxiHail.UpdatePasswordView({
                    model: this.model
                });
                this._password.render();
                this.$("#user-account-container").html(this._password.el);
            }

        },
        
        selectTab: function (tabName) {
            this.$('[data-tab=' + tabName + ']').addClass('active').siblings().removeClass('active');
            this.tab[tabName].apply(this);

        }
        
    });

}());
