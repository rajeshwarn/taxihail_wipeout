(function(TaxiHail){

    "use strict";

    TaxiHail.ManageRatesView = TaxiHail.TemplatedView.extend({
        render: function() {

            this.$el.html(this.renderTemplate());

            this.collection.each(this.renderItem, this);

            return this;
        },

        renderItem: function(rate) {

            new TaxiHail.RateItemView({
                model: rate
            }).render().$el.appendTo(this.$('ul'));
            
        }
    });


}(TaxiHail));