(function () {

    var View = TaxiHail.PaymentView = TaxiHail.TemplatedView.extend({

        events: {
            'change :input': 'onPropertyChanged',
            'click [data-action=destroy]': 'deleteCreditCard'
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
                    expirationYears: this.generateExpYears(),
                    isEditing: false,
                    isCreditCardMandatory: TaxiHail.parameters.isCreditCardMandatory
                });
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

        generateExpYears: function () {

            // Generate expiration years for the next 15 years
            var expYears = new Array(16);
            var currentYear = new Date().getFullYear();

            for (var i = 0; i <= 15; i++) {
                var expYear = currentYear + i;
                expYears[i] = { id: expYear, display: expYear }
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

        savechanges: function (form) {
            var cardNumber = this.model.get('cardNumber');
            var expMonth = this.model.get('expirationMonth');
            var formattedExpMonth = expMonth;
            if (formattedExpMonth < 10) {
                formattedExpMonth = "0" + expMonth;
            }

            var expYear = this.model.get('expirationYear');
            var cvv = this.model.get('cvv');

            // Tokenize credit card
            this.model.tokenize(cardNumber, formattedExpMonth, expYear, cvv)
	            .done(_.bind(function (tokenizedCard) {

                    // Set up request fields
	                this.model.set("token", tokenizedCard.cardOnFileToken);
                    this.model.set("last4Digits", tokenizedCard.lastFour);
	                this.model.set("creditCardCompany", tokenizedCard.cardType);
	                //this.model.set("expirationMonth", this.model.get('expirationMonth'));
	                //this.model.set("expirationYear", this.model.get('expirationYear'));
	                //this.model.set("nameOnCard", this.model.get('nameOnCard'));

	                // Update card on file
	                this.model.updateCreditCard()
			            .done(_.bind(function () {
	                        TaxiHail.auth.account.set('defaultCreditCard', 'tempId');
			                this.renderConfirmationMessage();
			            }, this))
			            .fail(_.bind(function () {
			                this.$(':submit').button('reset');
			            }, this));
	            }, this))
	            .fail(_.bind(function () {
	                this.$(':submit').button('reset');
	            }, this));
        },

        deleteCreditCard: function (e) {
            e.preventDefault();
            TaxiHail.confirm({
                title: this.localize('Delete'),
                message: this.localize('modal.payment.deleteCreditCard')
            }).on('ok', function() {
                this.model.deleteCreditCard();
                this.renderConfirmationMessage();
                this.model.attributes = {last4Digits: null};
            }, this);
        }
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());