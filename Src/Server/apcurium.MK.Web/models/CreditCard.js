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
            return $.ajax({
                type: 'DELETE',
                url: 'api/account/creditcards',
                dataType: 'json'
            });
        },

        tokenize: function (cardNumber, expMonth, expYear, cvv) {

            var d = $.Deferred();

            $.ajax({
                type: 'GET',
                url: 'api/payments/braintree/generateclienttoken'
            })
            .then(function (clientToken) {
                     var client = new braintree.api.Client({ clientToken: clientToken });
                     // Get nonce from SDK
                     client.tokenizeCard(
                         {
                             number: cardNumber,
                             expirationMonth: expMonth,
                             expirationYear: expYear,
                             cvv: cvv
                         },

                     function (err, nonce) {
                         // Tokenizing card by sending the nonce to server
                         $.ajax({
                             type: 'POST',
                             url: 'api/payments/braintree/tokenize',
                             dataType: 'json',
                             data: {
                                 paymentMethodNonce: nonce
                             }
                         }).then(d.resolve, d.reject);
                     });

            }, d.reject);

            return d.promise();
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