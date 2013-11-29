(function () {

    TaxiHail.directionInfo = _.extend({}, Backbone.Events, {
        getInfo: function (originLat, originLng, destinationLat, destinationLng, date) {

            var coordinates = {
                originLat: originLat,
                originLng: originLng,
                destinationLat: destinationLat,
                destinationLng: destinationLng,
                date: date
            }, tarifMode = TaxiHail.parameters.directionTarifMode, fmt = 'json';

            if (tarifMode != 'AppTarif') {
                return $.get('api/ibsfare/', coordinates, function () { }, fmt)
        .done(function (result) {
            console.log(result)
            if (tarifMode == 'Both' && result.price == 0)
                return $.get('api/directions/', coordinates, function () { }, fmt)
           .done(function (result) {
               result.callForPrice = (result.price > 100);
           });
        });
            } else {
                return $.get('api/directions/', coordinates, function () { }, fmt)
            .done(function (result) {
                result.callForPrice = (result.price > 100);
            });
            }
        }
    });
}());