(function(TaxiHail){

    "use strict";

    TaxiHail.ManageRulesView = TaxiHail.TemplatedView.extend({

        initialize: function() {
            this.collection.on('reset', this.render, this);
        },

        render: function() {

            this.$el.html(this.renderTemplate());

            this.collection.each(this.renderItem, this);

            return this;
        },

        renderItem: function(rate) {

            new TaxiHail.TariffItemView({
                model: rate
            }).render().$el.appendTo(this.$('tbody'));
            
        }
    });


}(TaxiHail));