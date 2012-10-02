(function () {
    var settingschanged = false;
    TaxiHail.UpdatePasswordView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=updatepassword]': 'updatepassword',
            'change :text': 'onPropertyChanged',
            'change :input': 'onPropertyChanged'
        },

        initialize: function () {

        },

        render: function () {
            this.$el.html(this.renderTemplate(this.model.toJSON()));
            
            this.$("#updatePasswordForm").validate({
                rules: {
                    password: "required",
                    newPassword: {
                        required: true,
                        equalTo: "#confirmPassword"
                    },
                    confirmPassword: {
                        required: true,
                        equalTo: "#newPassword"
                    }
                },
                messages: {
                    password : {
                        required : TaxiHail.localize('Password required'),
                    },
                    newPassword : {
                        required: TaxiHail.localize('Password required'),
                    },
                    confirmPassword: {
                        required: TaxiHail.localize('Password required'),
                        equalTo: TaxiHail.localize('Password are not the same')
                    }
                }, success: function(label) {
            }
            });

            return this;
        },

        onPropertyChanged: function (e) {
            e.preventDefault();
            
            var $input = $(e.currentTarget);

            this.model.set($input.attr("name"),  $input.val());
        },
        
        updatepassword: function (e) {
            e.preventDefault();
            if (this.$("#updatePasswordForm").valid()) {
                var accountId = this.model.get('id');
                    $.post('api/accounts/' + accountId + '/updatePassword', {
                        accountId: accountId,
                        currentPassword: this.model.get('password'),
                        newPassword: this.model.get('newPassword')
                    }, function () {
                        $("#notif-bar").html(TaxiHail.localize('Password updated.'));
                    }, 'json').fail(function (response) {
                        $("#notif-bar").html(TaxiHail.localize(response.statusText));
                    });
                }
        }
    });

}());