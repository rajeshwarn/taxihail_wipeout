(function () {

    TaxiHail.PrivacyPolicy = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/privacypolicy",

        save: function (privacypolicy) {
            return $.ajax({
                type: 'POST',
                url: this.urlRoot,
                data: JSON.stringify({
                    policy: privacypolicy
                }),
                contentType: 'application/json'
            });
        }
    });

}());