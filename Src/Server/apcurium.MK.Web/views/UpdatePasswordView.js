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
            
            $("updatePasswordForm").validate({
                rules: {
                    password: "required",
                    newPassword: "required",
                    confirmPassword: {
                        required: true,
                        minlength: 2,
                        equalTo: "#newPassword"
                    }
                },
                messages: {
                    confirmPassword: {
                        required: "Password required",
                        equalTo: "Password are not the same",
                        minlength: "Min length is 2"
                    }
                }
            });
            this.$('[data-action=updatepassword]').addClass('disabled');
            return this;
        },

        onPropertyChanged: function (e) {
            e.preventDefault();
            
            var $input = $(e.currentTarget);

            this.model.set($input.attr("name"),  $input.val());
            settingschanged = true;
            this.$('[data-action=updatepassword]').removeClass('disabled');

        },
        
        updatepassword: function (e) {
            e.preventDefault();
            var settings = this.model.get('settings');
            var accountId = this.model.get('id');
            // if (settings.isValid() ) {

            if (settingschanged) {

                $.post('api/accounts/' + accountId + '/updatePassword', {
                    accountId: accountId,
                    currentPassword: this.model.get('password'),
                    newPassword: this.model.get('newPassword')
                }, function() {
                    $("#notif-bar").html(TaxiHail.localize('Password updated.'));
                    this.$('[data-action=updatepassword]').addClass('disabled');

                    settingschanged = false;
                }, 'json').fail(function (response) {
                    $("#notif-bar").html(TaxiHail.localize('Error') + response.statusText); // on affiche le status text en attendant le mess jquery validate localized (todo)
                });
            }

        }
    });

}());