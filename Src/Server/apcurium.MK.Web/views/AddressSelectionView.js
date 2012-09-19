(function() {

    TaxiHail.AddressSelectionView = TaxiHail.TemplatedView.extend({

        initialize: function () {
            this.collection.on('reset', this.render, this);
        },

        render: function () {
            this.$el.html(this.renderTemplate());

            this.collection.each(this.renderItem, this);

            return this;
        },
        
        renderItem: function (model) {

            var itemView = new TaxiHail.AddressItemView({
                model: model
            });

            this.$('ul.address-list').append(itemView.render().el);
        }
    });

}());