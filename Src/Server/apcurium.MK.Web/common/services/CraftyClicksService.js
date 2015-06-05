// Crafty clicks address service

(function () {
    var ukPostcodeRegex = new RegExp("^[A-Za-z][A-Za-z]?[0-9][0-9]?[A-Za-z]?\\s?[0-9][A-Za-z][A-Za-z]$");
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

        isValidPostalCode: function (query) {
            return ukPostcodeRegex.test(query);
        }

    };
}());
