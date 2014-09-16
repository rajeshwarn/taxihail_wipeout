(function(){
    
    var Controller = TaxiHail.NotificationController = TaxiHail.Controller.extend({

        initialize: function () {
            this.templates = new TaxiHail.EmailTemplates();
            $.when(this.templates.fetch()).then(this.ready);
        },
      
        sendpushnotification: function () {
            return new TaxiHail.SendPushNotificationView();
        },

        sendtestemail: function () {
            return new TaxiHail.SendTestEmailView({
                model: this.templates
            });
        }
    });
}());