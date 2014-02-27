(function () {

    TaxiHail.TermsAndConditions = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/termsandconditions",

        save: function (termsandconditions) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot,
                data: JSON.stringify({
                    termsAndConditions: termsandconditions
                }),
                contentType: 'application/json'
            });
        }
    });

}());