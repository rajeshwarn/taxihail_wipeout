﻿(function () {

    TaxiHail.OrderItemView = TaxiHail.TemplatedView.extend({

        tagName: 'li',

        events: {
            'click [data-action=select-order]': 'selectOrder'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },

        selectOrder: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());