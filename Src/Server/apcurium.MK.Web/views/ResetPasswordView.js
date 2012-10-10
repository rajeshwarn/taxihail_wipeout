(function () {
    
    var View = TaxiHail.ResetPasswordView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        initialize: function () {

        },

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
               
        resetpassword : function (form) {
            TaxiHail.auth.resetPassword($(form).find('[name=email]').val())
                .done(_.bind(function() {
                    this.$(':submit').button('reset');
                    this.$("#notif-bar").html(TaxiHail.localize('resetPassword.reset'));
                    this.$("[name=email]").val("");
                }, this))
                .fail(_.bind(function (response) {
                    this.$(':submit').button('reset');
                    if (response.status == 404) {
                        this.$("#notif-bar").html(TaxiHail.localize('resetPassword.accountNotFound'));
                    }
                }, this));
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());