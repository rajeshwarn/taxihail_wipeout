(function () {

    TaxiHail.PaymentItemView = TaxiHail.TemplatedView.extend({

        tagName: 'li',


        events: {
            'click [data-action=select-payment]': 'selectPayment'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },

        selectPayment: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());