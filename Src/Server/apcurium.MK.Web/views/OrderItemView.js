(function () {

    TaxiHail.OrderItemView = TaxiHail.TemplatedView.extend({

        tagName: 'li',
        

        events: {
            'click [data-action=select-order]': 'selectOrder'
        },

        render: function () {
            var data = _.extend(this.model.toJSON(), {
                showOrderNumber:TaxiHail.parameters.showOrderNumber
            });
            this.$el.html(this.renderTemplate(data));

            return this;
        },

        selectOrder: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());