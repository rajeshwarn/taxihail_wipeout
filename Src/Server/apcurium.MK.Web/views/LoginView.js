(function () {

    var View = TaxiHail.LoginView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        events: {
            "click [data-action=fblogin]": "fblogin",
            "click [data-action=signup]": "gotosignup"
        },

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
                $alert.append($('<div />').text(this.localize(result.errorCode)));
            }
            _.each(result.errors, function (error) {
                $alert.append($('<div />').text(this.localize(error.errorCode)));
            }, this);
            this.$('.errors').html($alert);
        },
        
        fblogin : function (e) {
            e.preventDefault();
            TaxiHail.auth.fblogin();
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());