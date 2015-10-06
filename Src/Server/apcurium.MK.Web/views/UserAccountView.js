(function () {

    TaxiHail.UserAccountView = TaxiHail.TemplatedView.extend({

        events: {
            'click [data-role=profile-tabs] .active a': 'reloadActiveTab'
        },

        render: function () {
            
            var data = this.model.toJSON();
            _.extend(data, {
                isBraintreePrepaidEnabled: TaxiHail.parameters.isBraintreePrepaidEnabled
            });

            this.$el.html(this.renderTemplate(data));
            return this;
        },
        
        tab: {
            profile: function() {
                this._tabView = new TaxiHail.ProfileView({
                    model: this.model
                }).render();
                this.$("#user-account-container").html(this._tabView.el);
            },

            payment: function () {

                var creditCards = new TaxiHail.CreditCardCollection();
                creditCards.fetch({
                    url: 'api/account/creditcards',
                    success: _.bind(function (collection) {

                        var container = this.$("#user-account-container");
                        TaxiHail.showSpinner(container);

                        if (TaxiHail.parameters.maxNumberOfCreditCards < 2) {
                            // Take only the first credit card since we no longer support multiple cards per account
                            var creditCardInfo = new TaxiHail.CreditCard();

                        this._tabView = new TaxiHail.PaymentDetailView({
                            model: creditCardInfo,
                            parent: this
                        });
                      
                        } else {
                            this._tabView = new TaxiHail.PaymentView({
                                collection: collection,
                                model : this.model,
                                parent: this
                            });
                        }
                       
                        this._tabView.render();
                        this.$("#user-account-container").html(this._tabView.el);
                    }, this)

                });


                //creditCard.fetch({
                //    url: 'api/account/creditcards',
                //    success: _.bind(function (model) {
                //        var creditCardInfo = new TaxiHail.CreditCard();
                //        if (model.length > 0) {
                //            // Take only the first credit card since we no longer support multiple cards per account
                //            creditCardInfo = model.models[0];
                //        }

                //        this._tabView = new TaxiHail.PaymentDetailView({
                //            model: creditCardInfo,
                //            parent: this
                //        });
                //        this._tabView.render();
                //        this.$("#user-account-container").html(this._tabView.el);
                //    }, this)
                    
                //});
            },

            favorites: function(){
                var addresses = new TaxiHail.AddressCollection(),
                        view = this._tabView = new TaxiHail.FavoritesView({
                            collection: addresses
                        }),
                    favorites = new TaxiHail.AddressCollection(),
                    history = new TaxiHail.AddressCollection(),
                    $container = this.$("#user-account-container");

                TaxiHail.showSpinner($container);
                favorites.fetch({
                    url: 'api/account/addresses',
                    success: _.bind(function (collection, resp) {
                        history.fetch({
                            url: 'api/account/addresses/history',
                            success: _.bind(function (collection, resp) {
                                addresses.reset(favorites.models.concat(history.models));
                                $container.html(view.el);
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
                            collection: model,
                            parent: this
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

        },

        showOnlyActiveTab: function() {
            this.$('.active').siblings().addClass('hidden');
            this.$('.active').addClass('hidden');
        },

        reloadActiveTab: function (e) {
            if (e) {
                e.preventDefault();
            }
            var tabName = this.$('.active').data().tab;
            this.selectTab(tabName);
        }
    });

}());
