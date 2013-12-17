(function () {

    TaxiHail.directionInfo = _.extend({}, Backbone.Events, {
        getInfo: function (originLat, originLng, destinationLat, destinationLng, date) {

            var preferedPrice = null, tempPrice = null;

            var coordinates = {
                originLat: originLat,
                originLng: originLng,
                destinationLat: destinationLat,
                destinationLng: destinationLng,
                date: date
            }, tarifMode = TaxiHail.parameters.directionTarifMode, needAValidTarif = TaxiHail.parameters.directionNeedAValidTarif, fmt = 'json';

            function getDirectionInfoEvent() {

                var directionInfoDefer = $.Deferred();

                if (tarifMode != 'AppTarif') {
                    $.get('api/ibsfare/', coordinates, function () { }, fmt).then(function (result) {
                        if (result.price == 0 && tarifMode == "Both") {
                            $.get('api/directions/', coordinates, function () { }, fmt).done(function (resultGoogleBoth) {                                
                                directionInfoDefer.resolve(resultGoogleBoth);
                            });
                        } else                            
                        {
                            directionInfoDefer.resolve(result);
                        }
                    });

                } else {
                    $.get('api/directions/', coordinates, function () { }, fmt).then(function (resultGoogleAppTarif) {
                        directionInfoDefer.resolve(resultGoogleAppTarif);
                    });
                }
                
                return directionInfoDefer.promise();
            }

            return $.when(getDirectionInfoEvent()).done(
              function (result) {
                  if (needAValidTarif == true)
                  {
                      result.noEstimateFare == true;
                  }
                  result.callForPrice = (result.price > 100);
              }
            );
        }
    });
}());