(function () {

    TaxiHail.OrderItemView = TaxiHail.TemplatedView.extend({

        tagName: 'tr',
        

        events: {
            'click [data-action=select-order]': 'selectOrder'
        },

        render: function () {
            //this.$el.attrs('style', 'border-bottom: 1px solid #000;');
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },

        selectOrder: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());