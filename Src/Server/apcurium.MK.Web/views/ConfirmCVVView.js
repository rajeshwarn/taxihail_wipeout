(function () {

    var View = TaxiHail.ConfirmCVVView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        events: {
            'click [data-action=cancel]': 'cancel'
        },

        initialize: function () {
            _.bindAll(this, 'book', 'showErrors');
        },

        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);

            this.validate({
                rules: {
                    'cvv': {
                        required: true,
                        regex: /^[0-9]{3,4}$/
                    }
                },
                messages: {
                    'cvv': {
                        regex: TaxiHail.localize('error.InvalidCvv')
                    }
                },
                submitHandler: this.book
            });

            return this;
        },

        book: function (form) {
            var cvv = this.$("#inputCVV").val();
            this.model.set('cvv', cvv);

            this.model.save({}, {
                success: TaxiHail.postpone(function (model) {
                    // Wait for order to be created before redirecting to status
                    ga('send', 'event', 'button', 'click', 'book web', 0);
                    TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
                }, this),
                error: this.showErrors
            });
        },

        cancel: function (e) {
            e.preventDefault();
            this.model.destroyLocal();
            TaxiHail.app.navigate('', { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
        },

        showErrors: function (model, result) {
            this.$(':submit').button('reset');

            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }

            var $alert = $('<div class="alert alert-error" />');
            if (result.errorCode == "CreateOrder_RuleDisable") {
                $alert.append($('<div />').text(result.message));
            }
            else if (result.statusText) {
                $alert.append($('<div />').text(this.localize(result.statusText)));
            }
            else if (result.errors.length > 0) {
                _.each(result.errors, function (error) {
                    $alert.append($('<div />').text(this.localize(error.statusText)));
                }, this);
            } else {
                $alert.append($('<div />').text(result.message));
            }
           
            this.$('.errors').html($alert);
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}())