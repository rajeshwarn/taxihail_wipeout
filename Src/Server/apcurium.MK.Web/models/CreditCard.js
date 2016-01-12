(function () {

    TaxiHail.CreditCard = Backbone.Model.extend({

        urlRoot: TaxiHail.parameters.apiRoot + "/account/creditcards",

        updateCreditCard: function () {
            var updatedCreditCard = this.attributes;

            return $.ajax({
                type: 'POST',
                url: 'api/account/creditcards',
                data: updatedCreditCard,
                dataType: 'json'
            });
        },

        deleteCreditCard: function() {
            var result = $.ajax({
                type: 'DELETE',
                url: 'api/account/creditcards/' + this.attributes.creditCardId,
                dataType: 'json'
            });

            return result;
        },

        changeCreditCardLabel: function () {
            var updatedCreditCard = this.attributes;
            return $.ajax({
                type: 'POST',
                url: 'api/account/creditcard/updatelabel',
                data: updatedCreditCard,
                dataType: 'json'
            });
        },

        changeDefaultCreditCard: function () {
            return $.ajax({
                type: 'POST',
                url: 'api/account/creditcard/updatedefault',
                data: { "creditCardId": this.attributes.creditCardId },
                dataType: 'json'
            });
        },

        tokenize: function (cardNumber, expMonth, expYear, cvv) {

            var d = $.Deferred();

            $.ajax({
                type: 'GET',
                url: 'api/payments/braintree/generateclienttoken?format=json'
            })
            .then(function (generatedClientToken) {
                var client = new braintree.api.Client({ clientToken: generatedClientToken.clientToken });
                     // Get nonce from SDK
                     client.tokenizeCard(
                         {
                             number: cardNumber,
                             expirationMonth: expMonth,
                             expirationYear: expYear,
                             cvv: cvv
                         },

                     function(err, nonce) {
                         $.ajax({
                             type: 'POST',
                             url: 'api/payments/braintree/addpaymentmethod',
                             dataType: 'json',
                             data: {
                                 nonce: nonce,
                                 paymentMethod: 'CreditCard'
                             }
                         }).then(d.resolve, d.reject);
                     });

            }, d.reject);

            return d.promise();
        },

        updateSettings: function () {
            var settings = this.get('settings');

            var result = $.ajax({
                type: 'PUT',
                url: 'api/account/bookingsettings',
                data: JSON.stringify(settings),
                dataType: 'json',
                contentType: 'application/json; charset=UTF-8'
            });

            return result;
        },

        determineCompany: function(cardNumber) {
            var patterns = {
                visa: /^4[0-9]{12}(?:[0-9]{3})?$/,
                electron: /^(4026|417500|4405|4508|4844|4913|4917)\d+$/,
                mastercard: /^5[1-5][0-9]{14}$/,
                amex: /^3[47]\d{13}$/
            };

            if (patterns.visa.test(cardNumber)) {
                return { id: 0, display: "Visa" };
            } else if (patterns.mastercard.test(cardNumber)) {
                return { id: 1, display: "MasterCard" };
            } else if (patterns.amex.test(cardNumber)) {
                return { id: 2, display: "Amex" };
            } else if (patterns.electron.test(cardNumber)) {
                return { id: 3, display: "Visa Electron" };
            } else {
                return { id: 4, display: "Credit Card Generic" };
            }
        }
    });

}());