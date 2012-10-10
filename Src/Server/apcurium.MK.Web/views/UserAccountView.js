(function () {

    TaxiHail.UserAccountView = TaxiHail.TemplatedView.extend({

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            return this;
        },
        
        tab: {
            profile: function() {
                this._tabView = new TaxiHail.ProfileView({
                    model: this.model
                }).render();
                this.$("#user-account-container").html(this._tabView.el);
            },

            favorites: function(){
                var addresses = new TaxiHail.AddressCollection(),
                        view = this._tabView = new TaxiHail.FavoritesView({
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
                        this._tabView = new TaxiHail.OrderHistoryView({
                            collection:model
                        });
                        this._tabView.render();
                        this.$("#user-account-container").html(this._tabView.el);
                    }, this)
                    
                });
                
                
                
            },
            password: function () {
                this._tabView = new TaxiHail.UpdatePasswordView({
                    model: this.model
                });
                this._tabView.render();
                this.$("#user-account-container").html(this._tabView.el);
            }

        },
        
        selectTab: function (tabName) {
            this.$('[data-tab=' + tabName + ']').addClass('active').siblings().removeClass('active');
            this._tabView && this._tabView.remove();
            this.tab[tabName].apply(this);

        }
        
    });

}());
