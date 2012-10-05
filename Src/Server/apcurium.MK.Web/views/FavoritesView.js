(function () {

    TaxiHail.FavoritesView = TaxiHail.TemplatedView.extend({
        
        events :{
            'click [data-action=addfavorites]': 'addfavorites'
        },

        initialize: function () {
            this.collection.on('destroy reset change', this.render, this);
            this.collection.on('selected', this.edit, this);
        },

        render: function () {

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
            var view = new TaxiHail.AddFavoriteView(
                {
                    model: model
                });
            this.$el.html(view.render().el);
        },
        
        addfavorites: function (e) {
            e.preventDefault();
            this.model = new TaxiHail.Address();
            this.model.set('isNew', true);
            var view = new TaxiHail.AddFavoriteView(
                {
                    model : this.model
                });
            this.$el.html(view.render().el);
        }
        
    });

}());