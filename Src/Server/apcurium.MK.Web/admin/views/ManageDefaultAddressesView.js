(function () {

    TaxiHail.ManageDefaultAddressesView = TaxiHail.TemplatedView.extend({


        events: {
            'click [data-action=addfavorites]': 'addfavorites'
        },

        initialize: function () {
            this.collection.on('destroy  sync', TaxiHail.postpone(this.refresh, this), this);
            this.collection.on('selected', this.edit, this);

            this._addFavoriteView = null;
        },

        refresh: function () {
            var addresses;


            var favorites = new TaxiHail.CompanyDefaultAddressCollection();
            favorites.fetch({
                url: '../api/admin/addresses',
                success: _.bind(function (collection, resp) {
                            this.collection.reset(favorites.models, { silent: true });
                            this.$("#user-account-container").html(this.el);
                            this.render();
                        }, this)
            });
            this.$("#user-account-container").html(this.el);
        },

        render: function () {

            if (this._addFavoriteView) {
                this._addFavoriteView.remove();
                this._addFavoriteView = null;
            }

            this.$el.html(this.renderTemplate());
            var favorites = this.collection.filter(function(model) {
                return !model.get('isHistoric');
            });

            var $ul = this.$('ul');
            if (favorites.length) {
                _.each(favorites, function (model) {
                    this.renderItem(model, $ul[0]);
                }, this);
            }

            var $add = $('<a href="#" data-action=addfavorites>').addClass('new').text(TaxiHail.localize('favorites.add-new'));

            $ul.first().append($('<li>').append($add));

            return this;
        },

        renderItem: function (model, container) {

            var itemView = new TaxiHail.AddressItemView({
                model: model
            });
            $(container).append(itemView.render().el);
        },

        edit: function (model) {
            model.set('isNew', false);
            var view = this._addFavoriteView = new TaxiHail.AddFavoriteView({
                model: model,
                collection: this.collection,
                showPlaces:false
            });
            view.on('cancel', this.render, this);
            this.$el.html(view.render().el);
        },

        addfavorites: function (e) {
            e.preventDefault();
            this.model = new TaxiHail.CompanyDefaultAddress();
            this.model.set('isNew', true);
            var view = this._addFavoriteView = new TaxiHail.AddFavoriteView(
                {
                    model: this.model,
                    collection: this.collection,
                    showPlaces: false
                });
            view.on('cancel', this.render, this);
            this.$el.html(view.render().el);
        }

    });

}());