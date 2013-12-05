(function () {
    
    var View = TaxiHail.SendPushNotificationView = TaxiHail.TemplatedView.extend({

        urlRoot: TaxiHail.parameters.apiRoot + "/admin/pushnotifications",

        tagName: 'form',
        className: 'form-horizontal',
        
        events: {
            'click [data-action=sendpushnotification]': 'sendpushnotification'            
        },

        render: function () {
            this.$el.html(this.renderTemplate());
            return this;
        },
        
        sendpushnotification: function (e) {
            e.preventDefault();
            var email = this.$('[name=email]').val();
            var message = this.$('[name=message]').val();
           
            return $.ajax({
                type: 'POST',
                url: this.urlRoot + '/' + email,
                data: {
                    message: message
                },
                dataType: 'json',
                success: _.bind(function () {
                    this.$('.errors').text(TaxiHail.localize('sendPushNotificationSuccess'));
                }, this)
            }).fail(_.bind(function (xhr, textStatus, error) {
                if (xhr.statusText == "DirectoryNotFoundException")
                {
                    this.$('.errors').text(TaxiHail.localize('sendPushNotificationErrorHandleDirectory'));
                } else {
                    this.$('.errors').text(TaxiHail.localize(xhr.statusText));
                }                
            }), this);
        }
    });
}());