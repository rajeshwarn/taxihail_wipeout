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
            }).then(function(result) {
                if (result && result.length) {
                    _.each(result, function (address) {
                        // BUGFIX: All addresses have the same empty Guid as id
                        delete address.id;
                    });
                }
                defer.resolve(result);
            }, defer.reject);

            return defer.promise();
        },

        isValidPostalCode: function(query) {
            var value = query.replace(" ", "");

            return value.length == 7;
        }

    };
}());
