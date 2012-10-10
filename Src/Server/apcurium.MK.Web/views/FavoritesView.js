(function () {

    TaxiHail.FavoritesView = TaxiHail.TemplatedView.extend({
        
        events :{
            'click [data-action=addfavorites]': 'addfavorites'
        },

        initialize: function () {
            this.collection.on('destroy reset sync', TaxiHail.postpone(this.refresh, this), this);
            this.collection.on('selected', this.edit, this);
            
            this._addFavoriteView = null;
        },
        
        refresh: function () {
            var addresses;
            

            var favorites = new TaxiHail.AddressCollection();
            var history = new TaxiHail.AddressCollection();
            favorites.fetch({
                url: 'api/account/addresses',
                success: _.bind(function (collection, resp) {
                    history.fetch({
                        url: 'api/account/addresses/history',
                        success: _.bind(function (collection, resp) {
                            this.collection.reset(favorites.models.concat(history.models), {silent : true});
                            this.$("#user-account-container").html(this.el);
                            this.render();
                        }, this)
                    });
                }, this)
            });
            
        },

        render: function () {

            if(this._addFavoriteView) {
                this._addFavoriteView.remove();
                this._addFavoriteView = null;
            }

            this.$el.html(this.renderTemplate());
            var favorites = this.collection.filter(function (model) {
                return !model.get('isHistoric');
            }),
                history = this.collection.filter(function (model) {
                    return model.get('isHistoric');
                });

            var $ul = this.$('ul');
            if (favorites.length) {
                _.each(favorites, function(model) {
                    this.renderItem(model, $ul[0]);
                }, this);
            }

            var $add = $('<a href="#" data-action=addfavorites>').addClass('new').text(TaxiHail.localize('favorites.add-new'));

            $ul.first().append($('<li>').append($add));

            if (history.length) {
                _.each(history, function(model) {
                    this.renderItem(model, $ul[1]);
                }, this);
            } else {
                this.$el.append($('<li>').addClass('no-result').text(TaxiHail.localize('history.no-result')));
            }

            return this;
        },

        renderItem: function (model, container) {

            var itemView = new TaxiHail.AddressItemView({
                model: model
            });
            $(container).append(itemView.render().el);
        },
        
        edit:function (model) {
            var view = this._addFavoriteView = new TaxiHail.AddFavoriteView({
                model: model,
                collection : this.collection
            });
            view.on('cancel', this.render, this);
            this.$el.html(view.render().el);
        },
        
        addfavorites: function (e) {
            e.preventDefault();
            this.model = new TaxiHail.Address();
            var view = this._addFavoriteView = new TaxiHail.AddFavoriteView(
                {
                    model: this.model,
                    collection: this.collection
                });
            view.on('cancel', this.render, this);
            this.$el.html(view.render().el);
        }
        
    });

}());