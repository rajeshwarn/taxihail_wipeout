(function() {

    TaxiHail.AddressItemView = TaxiHail.TemplatedView.extend({
        
        tagName: 'li',
        
        events: {
            'click [data-action=select-address]': 'selectAddress'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },
        
        selectAddress: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());