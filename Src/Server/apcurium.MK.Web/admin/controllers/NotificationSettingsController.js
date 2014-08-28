(function(){

    var Controller = TaxiHail.NotificationSettingsController = TaxiHail.Controller.extend({
        initialize: function() {
            this.settings = new TaxiHail.NotificationSettings();
            $.when(this.settings.fetch()).then(this.ready);
        },

        index: function() {
            return new TaxiHail.ManageNotificationSettingsView({
                model: this.settings
            });
        }
    });
}());