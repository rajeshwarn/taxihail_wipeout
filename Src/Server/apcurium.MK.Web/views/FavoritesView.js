(function () {

    TaxiHail.FavoritesView = Backbone.View.extend({
        
        className: 'unstyled',
        tagName: 'ul',

        events :{
            'click [data-action=addfavorites]': 'addfavorites',
        },

        initialize: function () {
            this.collection.on('reset', this.render, this);
            this.collection.on('selected', this.edit, this);
        },

        render: function () {

            this.$el.empty();
            var favorites = this.collection.filter(function (model) {
                return !model.get('isHistoric');
            }),
                history = this.collection.filter(function (model) {
                    return model.get('isHistoric');
                });
            this.$el.append($('<h3>').addClass('table-title').text(TaxiHail.localize('Favorites Locations')));
            if (favorites.length) {
                _.each(favorites, this.renderItem, this);
            } 
                this.$el.append($('<li data-action=addfavorites>').addClass('no-result').text(TaxiHail.localize('favorites.add-new')));

            this.$el.append($('<h3>').addClass('table-title').text(TaxiHail.localize('Location History')));

            if (history.length) {
                _.each(history, this.renderItem, this);
            } else {
                this.$el.append($('<li>').addClass('no-result').text(TaxiHail.localize('history.no-result')));
            }

            return this;
        },

        renderItem: function (model) {

            var itemView = new TaxiHail.AddressItemView({
                model: model
            });
            this.$el.append(itemView.render().el);
        },
        
        edit:function (model) {
            model.set('isNew', false);
            var view = new TaxiHail.AddFavoriteView(
                {
                    model: model
                });
            this.$el.html(view.render().el);
        },
        
        addfavorites: function () {
            this.model = new TaxiHail.Address();
            this.model.set('isNew', true);
            var view = new TaxiHail.AddFavoriteView(
                {
                    model : this.model
                });
            this.$el.html(view.render().el);
        },
        
        editfavorites: function () {
            this.model.set('isNew', false);
            var view = new TaxiHail.AddFavoriteView(
                {
                    model: this.model
                });
            this.$el.html(view.render().el);
        },

    });

}());