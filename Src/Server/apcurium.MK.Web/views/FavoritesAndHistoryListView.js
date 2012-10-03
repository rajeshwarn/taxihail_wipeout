(function(){

    TaxiHail.FavoritesAndHistoryListView = Backbone.View.extend({

        className: 'unstyled',
        tagName: 'ul',

        initialize: function() {
            this.collection.on('reset', this.render, this);
        },

        render: function() {

            this.$el.empty();
            var favorites = this.collection.filter(function(model) {
                return !model.get('isHistoric');
            }),
                history = this.collection.filter(function(model) {
                return model.get('isHistoric');
            });

            if(favorites.length)
            {
                _.each(favorites, this.renderItem, this);
            } else {
                this.$el.append($('<li>').addClass('no-result').text(TaxiHail.localize('favorites.no-result')));
            }

            this.$el.append($('<li>').addClass('optgroup').text(TaxiHail.localize('History')));

            if(history.length)
            {
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
        }

    });

}());