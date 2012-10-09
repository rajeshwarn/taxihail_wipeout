(function () {

    TaxiHail.FavoritesView = TaxiHail.TemplatedView.extend({
        
        events :{
            'click [data-action=addfavorites]': 'addfavorites'
        },

        initialize: function () {
            this.collection.on('destroy reset sync', this.render, this);
            this.collection.on('selected', this.edit, this);
            
            this._addFavoriteView = null;
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
            model.set('isNew', false);
            var view = this._addFavoriteView = new TaxiHail.AddFavoriteView({
                model: model,
                collection : this.collection
            });
            view.on('cancel', this.render, this);
            this.$el.html(view.render().el);
            TaxiHail.app.navigate('favorites/edit');
        },
        
        addfavorites: function (e) {
            e.preventDefault();
            this.model = new TaxiHail.Address();
            this.model.on('sync', this.render, this);
            this.model.set('isNew', true);
            var view = this._addFavoriteView = new TaxiHail.AddFavoriteView(
                {
                    model: this.model,
                    collection: this.collection
                });
            view.on('cancel', this.render, this);
            this.$el.html(view.render().el);
            TaxiHail.app.navigate('favorites/add');
        }
        
    });

}());