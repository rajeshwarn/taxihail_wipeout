(function(){

    TaxiHail.AddressListView = Backbone.View.extend({

        tagName: 'ul',

        initialize: function() {
            this.collection.on('reset', this.render, this);
        },

        render: function() {

            this.$el.empty();
            this.collection.each(this.renderItem, this);

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