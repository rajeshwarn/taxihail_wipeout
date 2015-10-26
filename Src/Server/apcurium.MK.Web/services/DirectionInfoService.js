(function () {

    TaxiHail.directionInfo = _.extend({}, Backbone.Events, {
        getInfo: function (originLat, originLng, destinationLat, destinationLng, pickupZipCode, dropOffZipCode, vehicleTypeId, date, account) {
            
            var preferedPrice = null, tempPrice = null;

            var coordinates = {
                originLat: originLat,
                originLng: originLng,
                destinationLat: destinationLat,
                destinationLng: destinationLng,
                vehicleTypeId: vehicleTypeId,
                date: date
            }, tarifMode = TaxiHail.parameters.directionTarifMode, needAValidTarif = TaxiHail.parameters.directionNeedAValidTarif, fmt = 'json';

            function getDirectionInfoEvent() {
                var directionInfoDefer = $.Deferred();
                var tripDurationInSeconds = null;

                if (tarifMode == "Ibs_Distance") {
                    var distance = null;
                    $.ajax({
                        url: 'api/directions/',
                        data: coordinates,
                        dataType: fmt,
                        success: function (result, status) {
                            tripDurationInSeconds = result.tripDurationInSeconds;
                            distance = result.distance;
                        },
                        async: false
                    });

                    $.get('api/ibsdistance?Distance={0}&WaitTime={1}&StopCount={2}&PassengerCount={3}&AccountNumber={4}&CustomerNumber={5}&VehicleType={6}'.format(
                           distance,  
                           (tripDurationInSeconds != null)
                            ? tripDurationInSeconds
                            : 0,
                            0,
                            0,
                            (account != null)
                            ? account
                            : '',
                            0,
                            vehicleTypeId),
                        function() {}, fmt).then(function(result) {
                        });
                }
                else if (tarifMode != 'AppTarif') {

                    $.ajax({
                        url: 'api/directions/',
                        data: coordinates,
                        dataType: fmt,
                        success: function (result,status) {
                            tripDurationInSeconds = result.tripDurationInSeconds;
                        },
                        async: false
                    });

                    $.get('api/ibsfare?PickupLatitude={0}&PickupLongitude={1}&DropoffLatitude={2}&DropoffLongitude={3}&PickupZipCode={4}&DropoffZipCode={5}&AccountNumber={6}&CustomerNumber={7}&TripDurationInSeconds={8}&VehicleType={9}'.format(
                            coordinates.originLat, coordinates.originLng, coordinates.destinationLat, coordinates.destinationLng, pickupZipCode, dropOffZipCode,
                            (account != null)
                                ? account
                                : '',
                            0,
                            (tripDurationInSeconds != null)
                                ? tripDurationInSeconds
                                : '',
                            vehicleTypeId),
                        function () { }, fmt).then(function (result) {
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
                    $.get('api/directions/', coordinates, function () {}, fmt).then(function (resultGoogleAppTarif) {
                        directionInfoDefer.resolve(resultGoogleAppTarif);
                    });
                }
                
                return directionInfoDefer.promise();
            }

            return $.when(getDirectionInfoEvent()).done(
              function (result) {
                  result.noFareEstimate = (result.price == 0);
                  result.callForPrice = (result.price > TaxiHail.parameters.maxFareEstimate);
              }
            );
        },

        getAssignedEta: function (orderId, vehicleLat, vehicleLng) {
            return $.get(TaxiHail.parameters.apiRoot + '/directions/eta', { orderId: orderId, vehicleLat: vehicleLat, vehicleLng: vehicleLng }, function () { }, 'json');
	    },
        
        getEta: function (originLat, originLng) {

            var coordinates = {
                originLat: originLat,
                originLng: originLng,
            }, fmt = 'json';

            function getDirectionInfoEvent() {

                var directionInfoDefer = $.Deferred();

                    $.get('api/directions/', coordinates, function () { }, fmt).then(function (resultGoogleAppTarif) {
                        directionInfoDefer.resolve(resultGoogleAppTarif);
                    });
                
                return directionInfoDefer.promise();
            }

            return $.when(getDirectionInfoEvent()).done();
        }
    });
}());