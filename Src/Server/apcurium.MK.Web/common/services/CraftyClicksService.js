// Crafty clicks address service

(function () {

    TaxiHail.craftyclicks = {

        getCraftyClicksAdresses: function (postcode) {
            var defer = $.Deferred();

            $.ajax({
                url: TaxiHail.parameters.apiRoot + "/addressFromPostalCode",
                type: "POST",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                data: JSON.stringify({ postalCode: postcode })
            }).then(function (result) {
                TaxiHail.cleanupAddressesResult(result);
                defer.resolve(result);
            }, defer.reject);

            return defer.promise();
        },

        isValidPostalCode: function(query) {
            var value = query.replace(" ", "");

            return value.length >= 5 && value.length <= 7;
        }

    };
}());
