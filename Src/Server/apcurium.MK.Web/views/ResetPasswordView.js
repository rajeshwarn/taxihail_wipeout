(function () {
    
    var View = TaxiHail.ResetPasswordView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        render: function () {
            this.$el.html(this.renderTemplate());

            this.validate({
                rules: {
                    email: {
                        required: true,
                        email: true
                    }
                },
                messages: {
                    email: {
                        required: TaxiHail.localize('error.EmailRequired'),
                        email: TaxiHail.localize('error.NotEmailFormat')
                    }
                },
                submitHandler: this.resetpassword
            });
        
            return this;
        },

        renderConfirmationMessage: function() {
            var view = new TaxiHail.AlertView({
                message: TaxiHail.localize('resetPassword.emailSent'),
                type: 'success'
            });
            view.on('ok', this.render, this);
            this.$('.well').html(view.render().el);
        },
               
        resetpassword : function (form) {
            TaxiHail.auth.resetPassword($(form).find('[name=email]').val())
                .done(_.bind(function() {

                    this.renderConfirmationMessage();


                }, this))
                .fail(_.bind(function (response) {
                    this.$(':submit').button('reset');
                    if (response.status == 404) {
                        var alert = new TaxiHail.AlertView({
                            message: TaxiHail.localize('resetPassword.accountNotFound'),
                            type: 'error'
                        });
                        alert.on('ok', alert.remove, alert);
                        this.$('.errors').html(alert.render().el);
                    }
                }, this));
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());