(function () {

    TaxiHail.LoginView = TaxiHail.TemplatedView.extend({

        events: {
            "submit form": 'onSubmit',
            "click [data-action=resetpassword]" : "resetpassword"
        },

        render: function () {

            this.$el.html(this.renderTemplate());

            return this;

        },
        
        showConfirmationMessage: function() {
            this.$('#alert')
                .addClass('alert')
                .addClass('alert-success')
                .html(this.localize('signup.confirmation'));
        },

        onSubmit: function (e) {
            var $email = this.$('[name=email]'),
                $password = this.$('[name=password]'),
                isValid = true;
            
            e.preventDefault();
            
            // Validate field values
            if (!$email.val()) {
                $email.parents('.control-group').addClass('error');
                isValid = false;
            }
            
            if (!$password.val()) {
                $password.parents('.control-group').addClass('error');
                isValid = false;
            }

            if (isValid) {
                this.$(':submit').button('loading');
                TaxiHail.auth.login(this.$('[name=email]').val(), this.$('[name=password]').val())
                    .fail(_.bind(function(response) {
                        this.showErrors(this.model, response);
                    }, this));
            }

        },

        showErrors: function (model, result) {
            this.$(':submit').button('reset');
            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }
            var $alert = $('<div class="alert alert-error" />');
            if (result.message) {
                $alert.append($('<div />').text(this.localize(result.errorCode)));
            }
            _.each(result.errors, function (error) {
                $alert.append($('<div />').text(this.localize(error.errorCode)));
            }, this);
            this.$('.errors').html($alert);
        },
        
        resetpassword : function (e) {
            e.preventDefault();
            TaxiHail.app.navigate('resetpassword', { trigger: true });
        }

    });

}());