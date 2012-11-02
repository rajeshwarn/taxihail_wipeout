(function () {

    TaxiHail.CompanySettings = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/settings",

        batchSave: function (settings) {

            return $.ajax({
                type: 'POST',
                url: this.urlRoot,
                data: JSON.stringify({
                    appSettings: settings
                }),
                contentType: 'application/json'
            });
        }
    });

}());