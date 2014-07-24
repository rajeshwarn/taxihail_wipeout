(function () {

    var View = TaxiHail.LoginView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        events: {
            "click [data-action=fblogin]": "fblogin",
            "click [data-action=twlogin]": "twlogin",
            "click [data-action=signup]": "gotosignup"
        },

        options: {
            returnUrl: ''
        },

        render: function () {

            this.$el.html(this.renderTemplate());
            
            if (TaxiHail.parameters.facebookEnabled == false) {
                this.$('[data-action=fblogin]').addClass('hidden');
            }

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
        
        showConfirmationMessage: function () {
            var $confirmationMessage = this.localize('signup.confirmation');
            if (TaxiHail.parameters.accountActivationDisabled) {
                $confirmationMessage = this.localize('signup.confirmationWithoutActivation');
            }

            this.$('#alert')
                .addClass('alert')
                .addClass('alert-success')
                .html($confirmationMessage);
        },

        onsubmit: function (form) {
            var $email = this.$('[name=email]'),
                $password = this.$('[name=password]');
            TaxiHail.auth.login($email.val(), $password.val(), this.options.returnUrl)
                .fail(_.bind(this.showErrors, this, this.model));
            if (!this.options.returnUrl) {
                this.options.returnUrl = '';
                }

        },
        
        gotosignup: function (e) {
            e.preventDefault();
            if (!this.options.returnUrl) {
                TaxiHail.app.navigate('signup', { trigger: true });
            } else {
                TaxiHail.app.navigate('signup/' + this.options.returnUrl, { trigger: true });
            }
        },

        showErrors: function (model, result) {
            this.$(':submit').button('reset');
            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }
            var $alert = $('<div class="alert alert-error" />');
            if (result.message) {
                var message = this.localize(result.errorCode)
                    .replace('{{ApplicationName}}', TaxiHail.parameters.applicationName)
                    .replace('{{PhoneNumber}}', TaxiHail.parameters.defaultPhoneNumber);
                $alert.append($('<div />').text(message));
            }
            _.each(result.errors, function (error) {
                $alert.append($('<div />').text(this.localize(error.errorCode)));
            }, this);
            this.$('.errors').html($alert);
        },
        
        fblogin : function (e) {
            e.preventDefault();
            var url = this.options.returnUrl;
            if (url) {
                FB.login(_.bind(function () { TaxiHail.auth.fblogin(url); }, { scope: 'email' }, this));
            } else {
                FB.login(function () { TaxiHail.auth.fblogin(); }, { scope: 'email' });
            }
        },
        
        twlogin : function (e) {
            e.preventDefault();
            var url = this.options.returnUrl;
            if (url) {
                TaxiHail.auth.twlogin(url);
            } else {
                TaxiHail.auth.twlogin();
            }
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());