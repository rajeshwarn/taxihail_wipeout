(function () {

    var View = TaxiHail.PaymentDetailView = TaxiHail.TemplatedView.extend({

        events: {
            'change :input': 'onPropertyChanged',
            'click [data-action=destroy]': 'deleteCreditCard',
            'click [data-action=cancel]': 'cancel',
        },

        initialize: function () {

            _.bindAll(this, 'savechanges');
        },

        render: function () {

            var expMonths = [
                { id: 1, display: this.localize("January") },
                { id: 2, display: this.localize("February") },
                { id: 3, display: this.localize("Mars") },
                { id: 4, display: this.localize("April") },
                { id: 5, display: this.localize("May") },
                { id: 6, display: this.localize("June") },
                { id: 7, display: this.localize("July") },
                { id: 8, display: this.localize("August") },
                { id: 9, display: this.localize("September") },
                { id: 10, display: this.localize("October") },
                { id: 11, display: this.localize("November") },
                { id: 12, display: this.localize("December") }
            ];
            
            var isEditing = this.model != null && this.model.get('last4Digits') != null;
            var creditCard = {};

            if (isEditing) {
                creditCard = this.model.toJSON();
                var cardNumber = "************" + creditCard.last4Digits;

                _.extend(creditCard, {
                    cardNumber: cardNumber,
                    expirationMonths: expMonths,
                    expirationYears: this.generateExpYears(),
                    isEditing: true,
                    isCreditCardMandatory: TaxiHail.parameters.isCreditCardMandatory
                });
            } else {
                _.extend(creditCard, {
                    expirationMonths: expMonths,
                    expirationYears: this.generateExpYears(true),
                    isEditing: false,
                    isCreditCardMandatory: TaxiHail.parameters.isCreditCardMandatory,
                    label : "Personal"
                });
                this.model.set('label', "Personal");
                this.model.set('expirationMonth', 1);
            }

            this.$el.html(this.renderTemplate(creditCard));

            this.validate({
                rules: {
                    cardName: "required",
                    cvv: "required",
                    cardNumber : {
                        required: true,
                        creditcard: true
                    }
                },
                messages: {
                    name: {
                        required: TaxiHail.localize('error.NameRequired')
                    }
                },
                submitHandler: this.savechanges
            });

            return this;
        },

        onPropertyChanged : function (e) {
            var $input = $(e.currentTarget);

            var name = $input.attr("name");
            var value = $input.val();

            this.model.set(name, value);

            this.$(':submit').removeClass('disabled');
        },

        generateExpYears: function (setYear) {

            // Generate expiration years for the next 15 years
            var expYears = new Array(16);
            var currentYear = new Date().getFullYear();

            for (var i = 0; i <= 15; i++) {
                var expYear = currentYear + i;
                expYears[i] = { id: expYear, display: expYear }
            }

            if (setYear) {
                this.model.set('expirationYear', expYears[0].id);
            }

            return expYears;
        },

        renderConfirmationMessage: function () {
            var view = new TaxiHail.AlertView({
                message: this.localize('Changes were saved'),
                type: 'success'
            });
            view.on('ok', this.render, this);

            this.$el.html(view.render().el);
        },


        renderErrorMessage: function (message) {
            var errorMessage = message;
            if (!errorMessage) {
                errorMessage = TaxiHail.localize("TokenizeGenericError");
            }

            var alert = new TaxiHail.AlertView({
                message: errorMessage,
                type: 'error'
            });

            alert.on('ok', alert.remove, alert);
            this.$('.errors').html(alert.render().el);
        },

        renderDeleteCreditCardErrorMessage: function () {
            var alert = new TaxiHail.AlertView({
                message: TaxiHail.localize("error.CreditCardNotDeleted"),
                type: 'error'
            });

            alert.on('ok', alert.remove, alert);
            this.$('.errors').html(alert.render().el);
        },

        savechanges: function (form) {
            var cardNumber = this.model.get('cardNumber');
            var expMonth = this.model.get('expirationMonth');
            var label = this.model.get('label');

            var formattedExpMonth = expMonth;
            if (formattedExpMonth < 10) {
                formattedExpMonth = "0" + expMonth;
            }

            var expYear = this.model.get('expirationYear');
            var cvv = this.model.get('cvv');

            var now = new Date();
            var exp = new Date(expYear, expMonth - 1, 1, 23, 59, 59);
            exp.setMonth(exp.getMonth() + 1);           // add a month
            exp.setDate(exp.getDate() - 1);             //remove one day

            if (exp < now) {
                this.$(':submit').button('reset');
                this.renderErrorMessage(TaxiHail.localize("ExpiredCreditCardError"));
                return;
            }

            // Tokenize credit card
            this.model.tokenize(cardNumber, formattedExpMonth, expYear, cvv)
	            .done(_.bind(function (tokenizedCard) {

	                if (tokenizedCard.isSuccessful) {
	                    // Set up request fields
	                    this.model.set("token", tokenizedCard.cardOnFileToken);
	                    this.model.set("last4Digits", tokenizedCard.lastFour);
	                    this.model.set("creditCardCompany", tokenizedCard.cardType);
	                    this.model.set("label", label);

	                    // Update card on file
	                    this.model.updateCreditCard()
                            .done(_.bind(function () {
                                TaxiHail.auth.account.set('defaultCreditCard', 'tempId');

                                if (this.isOnBookingFlow()) {
                                    var currentOrder = TaxiHail.orderService.getCurrentOrder();

                                    if (TaxiHail.parameters.askForCVVAtBooking) {
                                        currentOrder.set('cvv', cvv);
                                    }

                                    // Wait for credit card to be added to profile
                                    setTimeout(function () { }, 1000);

                                    currentOrder.save({}, {
                                        success: TaxiHail.postpone(function (model) {
                                            // Wait for response before doing anything
                                            ga('send', 'event', 'button', 'click', 'book web', 0);

                                            TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
                                        }, this),
                                        error: this.showErrors
                                    });
                                } else {
                                    this.renderConfirmationMessage();
                                }
                            }, this))
                            .fail(_.bind(function () {
                                this.$(':submit').button('reset');
	                            this.renderErrorMessage();
	                    }, this));
	                } else {
	                    this.$(':submit').button('reset');

	                    var alert = new TaxiHail.AlertView({
	                        message: TaxiHail.localize("TokenizeInvalidCardInfos"),
	                        type: 'error'
	                    });

	                    alert.on('ok', alert.remove, alert);
	                    this.$('.errors').html(alert.render().el);
	                }
	            }, this))
	            .fail(_.bind(function () {
	                this.$(':submit').button('reset');
	                this.renderErrorMessage();
	            }, this));
        },


        isOnBookingFlow: function () {
             var currentNode = $(location).attr('hash');

            return currentNode == '#confirmationbook/payment';
        },

        isOnPaymentFlow: function () {
            var currentNode = $(location).attr('hash');

            return currentNode == '#useraccount/payment';
        },

        cancel: function(e) {
            if (this.isOnBookingFlow()) {
                TaxiHail.app.navigate('confirmationbook', { trigger: true });
                e.preventDefault();
            }

            e.preventDefault();
            this.model.set(this.model.previousAttributes);
            this.trigger('cancel', this);
        },

        deleteCreditCard: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('Delete'),
                message: this.localize('modal.payment.deleteCreditCard')
            }).on('ok', function() {
                this.model.deleteCreditCard()
                    .done(_.bind(function() {
                        this.renderConfirmationMessage();
                        this.model.attributes = { last4Digits: null };
                    }, this))
                    .fail(_.bind(function () {
                        this.renderDeleteCreditCardErrorMessage();
                    }, this));
                
            }, this);
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());