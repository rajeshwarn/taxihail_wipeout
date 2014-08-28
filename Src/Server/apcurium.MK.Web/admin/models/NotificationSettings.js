(function () {

    TaxiHail.NotificationSettings = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/settings/notifications",

        batchSave: function (settings) {
            return $.ajax({
                type: 'POST',
                url: this.urlRoot,
                data: JSON.stringify({
                    notificationSettings: settings
                }),
                contentType: 'application/json'
            });
        }
    });

}());