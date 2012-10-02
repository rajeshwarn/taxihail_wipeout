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

            this.$("#resetPasswordForm").validate({
                rules: {
                    email: {
                        required: true,
                        email: true
                    }
                },
                messages: {
                    email: {
                        required: TaxiHail.localize('error.EmailRequired'),
                        email: TaxiHail.localize('error.NotEmailFormat'),
                    }
                },
                success: function(label) {
                }
            });
        
            return this;
        },
        
        onPropertyChanged: function (e) {
            e.preventDefault();
            
            var $input = $(e.currentTarget);

            email = $input.val();
        },
        
        resetpassword : function (e) {
            e.preventDefault();
            if (this.$("#resetPasswordForm").valid()) {
                $.post('api/account/resetpassword/' + email, {
                emailAddress: email
            }, function () {
                $("#notif-bar").html(TaxiHail.localize('resetPassword.emailSent'));
                $("#email").val("");
            }, 'json').fail(function (response) {
                if (response.status == 404) {
                    $("#notif-bar").html(TaxiHail.localize('resetPassword.accountNotFound'));
                }
                //$("#notif-bar").html(TaxiHail.localize(response.statusText));
            });
            }
            
        }

    });

}());