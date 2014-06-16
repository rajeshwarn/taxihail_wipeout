(function() {

    TaxiHail.AccountChargeItemView = TaxiHail.TemplatedView.extend({
        
        tagName: 'li',
        
        events: {
            'click [data-action=select-account]': 'selectAccount'
        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));

            return this;
        },
        
        selectAccount: function (e) {
            e.preventDefault();

            this.model.trigger('selected', this.model, this.model.collection);
        }

    });

}());