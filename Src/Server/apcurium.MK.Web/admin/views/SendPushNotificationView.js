(function () {
    
    var View = TaxiHail.SendPushNotificationView = TaxiHail.TemplatedView.extend({

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
                url: '../api/admin/pushnotifications/' + email,
                data: {
                    message: message
                },
                dataType: 'json',
                success: _.bind(function () {
                    this.$('.errors').text(TaxiHail.localize('sendPushNotificationSuccess'));
                }, this)
            }).fail(_.bind(function (e) {
                this.$('.errors').text(TaxiHail.localize('sendPushNotificationError'));
            }), this);
        }
    });
}());