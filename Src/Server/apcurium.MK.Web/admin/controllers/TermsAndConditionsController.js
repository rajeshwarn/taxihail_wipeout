(function(){

    var Controller = TaxiHail.TermsAndConditionsController = TaxiHail.Controller.extend({
        initialize: function() {
            this.termsAndConditions = new TaxiHail.TermsAndConditions();
            $.when(this.termsAndConditions.fetch())
                .then(this.ready);
        },

        index: function() {
            return new TaxiHail.UpdateTermsAndConditionsView({
                model: this.termsAndConditions
            });
        }
    });
}());