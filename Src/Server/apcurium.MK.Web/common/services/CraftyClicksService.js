// Crafty clicks address service

(function () {

    TaxiHail.craftyclicks = {

        getCraftyClicksAdresses: function (postcode) {

            if (TaxiHail.parameters.craftyclicksapikey)
            {
                var defer = $.Deferred();

                $.ajax({
                    url: "http://pcls1.craftyclicks.co.uk/json/",
                    type: "POST",
                    contentType: "application/json; charset=utf-8",
                    dataType: "json",
                    data: JSON.stringify({ postcode: postcode, key: TaxiHail.parameters.craftyclicksapikey, response: "data_formatted", sort: "asc", include_geocode: true })
                }).then(defer.resolve, defer.reject);

                return defer.promise();
            }

            return null;
        }
    };
}());
