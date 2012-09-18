(function () {

    TaxiHail.LoginView = TaxiHail.TemplatedView.extend({

        events: {
            "submit form": 'onSubmit'
        },

        render: function () {

            this.$el.html(this.renderTemplate());

            return this;

        },

        onSubmit: function (e) {
            e.preventDefault();

            TaxiHail.auth.login(this.$('[name=email]').val(), this.$('[name=password]').val())
                .fail(_.bind(function (response) {
                    this.showErrors(this.model, response);
                }, this));

        },

        showErrors: function (model, result) {
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

}());