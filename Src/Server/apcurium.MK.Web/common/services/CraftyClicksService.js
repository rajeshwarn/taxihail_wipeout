// Crafty clicks address service

(function () {

    TaxiHail.craftyclicks = {

        getCraftyClicksAdresses: function (postcode) {

            if (TaxiHail.parameters.craftyclicksapikey)
            {
                var defer = $.Deferred();

                $.ajax({
                    url: "http://pcls1.craftyclicks.co.uk/json/rapidaddress",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ postcode: postcode, key: TaxiHail.parameters.craftyclicksapikey, response: "data_formatted", sort: "asc", include_geocode: true })
                }).then(defer.resolve, defer.reject);

                return defer.promise();
            }

            return null;
        },

        toAddress: function (craftyClicksAddress) {

            if (craftyClicksAddress.error_code)
            {
                return new Array();
            }

            var collection = new Array();

            for (var index = 0; index < craftyClicksAddress.delivery_points.length; index++) {
                var address = craftyClicksAddress.delivery_points[index];

                var addressItem = new TaxiHail.Address({
                    zipcode: craftyClicksAddress.postcode,
                    friendlyName: address.line_1,
                    fullAddress: address.line_1 + ", " + craftyClicksAddress.town + ", " + craftyClicksAddress.postcode,
                    city: craftyClicksAddress.town,
                    longitude: craftyClicksAddress.geocode.lng,
                    latitude: craftyClicksAddress.geocode.lat,
                    addressType: "craftyclicks"
                });

                collection.push(addressItem);
            }

            return collection;
        }
    };
}());
