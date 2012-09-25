(function(){

    TaxiHail.AddressListView = Backbone.View.extend({

        tagName: 'ul',

        initialize: function() {
            this.$el.addClass('unstyled');
            this.collection.on('reset', this.render, this);
        },

        render: function() {

            this.$el.empty();
            if(this.collection.length)
            {
                this.collection.each(this.renderItem, this);
            } else {
                this.$el.append($('<li>').addClass('no-result').text(TaxiHail.localize('search.no-result')));
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