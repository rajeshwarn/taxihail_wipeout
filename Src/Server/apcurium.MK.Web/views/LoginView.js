(function () {

    var View = TaxiHail.LoginView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        options: {
            returnUrl: ''
        },

        render: function () {

            this.$el.html(this.renderTemplate());

            this.validate({
                rules: {
                    email: {
                        required:true
                    },
                    password: {
                        required: true
                    }
                },
                messages: {
                    email: {
                        required: TaxiHail.localize('error.EmailRequired')
                    },
                    password: {
                        required: TaxiHail.localize('Password required')
                    }
                },
                submitHandler: this.onsubmit
            });

            return this;

        },
        
        showConfirmationMessage: function() {
            this.$('#alert')
                .addClass('alert')
                .addClass('alert-success')
                .html(this.localize('signup.confirmation'));
        },

        onsubmit: function (form) {
            var $email = this.$('[name=email]'),
                $password = this.$('[name=password]');
            
            TaxiHail.auth.login($email.val(), $password.val(), this.options.returnUrl)
                .fail(_.bind(this.showErrors, this, this.model));

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
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());