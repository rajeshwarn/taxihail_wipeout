(function () {
    var email;
    TaxiHail.ResetPasswordView = TaxiHail.TemplatedView.extend({
        events: {
            "click [data-action=resetpassword]": "resetpassword",
            'change :input': 'onPropertyChanged'
        },

        initialize: function () {

        },

        render: function () {
            this.$el.html(this.renderTemplate());

        
            return this;
        },
        
        onPropertyChanged: function (e) {
            e.preventDefault();
            
            var $input = $(e.currentTarget);

            email = $input.val();
        },
        
        resetpassword : function (e) {
            e.preventDefault();
            if (email) {
                $.post('api/account/resetpassword/' + email, {
                emailAddress: email
            }, function () {
                $("#notif-bar").html(TaxiHail.localize('An email has been sent to you with confirmation link.'));
            }, 'json').fail(function (response) {
                $("#notif-bar").html(TaxiHail.localize(response.statusText));
            });
            }
            
        }

    });

}());