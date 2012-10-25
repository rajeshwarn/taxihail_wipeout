(function(TaxiHail){

    "use strict";

    TaxiHail.ManageRatesView = TaxiHail.TemplatedView.extend({

        initialize: function() {
            this.collection.on('reset', this.render, this);
        },

        render: function() {

            this.$el.html(this.renderTemplate());

            this.collection.each(this.renderItem, this);

            return this;
        },

        renderItem: function(rate) {

            new TaxiHail.RateItemView({
                model: rate
            }).render().$el.appendTo(this.$('tbody'));
            
        }
    });


}(TaxiHail));