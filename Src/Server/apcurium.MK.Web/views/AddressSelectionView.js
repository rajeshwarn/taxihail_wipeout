(function() {

    TaxiHail.AddressSelectionView = TaxiHail.TemplatedView.extend({

        events: {
            'click .nav-tabs li>a': 'selectTab'
        },

        initialize: function () {
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            this.show.favorites.call(this);

            return this;
        },
        
        selectTab: function(e) {
            e.preventDefault();

            var tabName = $(e.currentTarget).parent('li').data().tab;

            this.$('li').removeClass('active');
            $(e.currentTarget).parent('li').addClass('active');

            this.show[tabName].call(this);

        },

        show: {
            favorites: function() {

                var addresses = new TaxiHail.AddressCollection(),
                    view = new TaxiHail.FavoriteAddressesView({
                        collection: addresses
                    });

                addresses.fetch({
                    url: 'api/account/addresses'
                });

                addresses.on('selected', function (model, collection) {
                    this.trigger('selected', model, collection);
                }, this);

                this.$('.tab-content').html(view.el);
            },

            search: function() {
                this.$('.tab-content').html(new TaxiHail.AddressSearchView().render().el);
            },

            places: function() {
                this.$('.tab-content').empty();
            }

        }

    });

}());