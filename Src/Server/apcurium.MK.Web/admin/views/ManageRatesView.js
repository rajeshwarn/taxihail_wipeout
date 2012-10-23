(function(TaxiHail){

    "use strict";

    TaxiHail.ManageRatesView = TaxiHail.TemplatedView.extend({
        render: function() {

            this.$el.html(this.renderTemplate());

            return this;
        }
    });


}(TaxiHail));