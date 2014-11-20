(function(){

    var Controller = TaxiHail.PrivacyPolicyController = TaxiHail.Controller.extend({
        initialize: function() {
            this.privacyPolicy = new TaxiHail.PrivacyPolicy();
            $.when(this.privacyPolicy.fetch())
                .then(this.ready);
        },

        index: function() {
            return new TaxiHail.UpdatePrivacyPolicyView({
                model: this.privacyPolicy
            });
        }
    });
}());