(function () {

    TaxiHail.CreditCard = Backbone.Model.extend({

        urlRoot: TaxiHail.parameters.apiRoot + "/account/creditcards",

        updateCreditCard: function () {
            var updatedCreditCard = this;

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

        tokenize: function () {

            return $.ajax({
                type: 'GET',
                url: 'api/payments/braintree/generateclienttoken',
                dataType: 'json',
                success: _.bind(function (clientToken) {
                    //braintree.setup(clientToken, "<integration>", options);
                    //var client = new braintree.api.Client({ clientToken: clientToken });
                    //client.tokenizeCard({ number: "4111111111111111", expirationDate: "10/20" }, function (err, nonce) {
                    //    // Send nonce to your server
                    //    return nonce;
                    //});
                }, this)
            }).fail(_.bind(function (e) {
                this.$('.errors').text(e);
            }), this);
        },

        determineCompany: function(cardNumber) {
            //var visaPattern = new RegExp("/^4\d{3}-?\d{4}-?\d{4}-?\d");
            //var mastercardPattern = new RegExp("^5[1-5][0-9]{14}$");
            //var amexPattern = new RegExp("/^3[47]\d{13}$/");

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