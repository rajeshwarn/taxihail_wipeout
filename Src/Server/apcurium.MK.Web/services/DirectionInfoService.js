(function () {

    TaxiHail.directionInfo = _.extend({}, Backbone.Events, {
        getInfo: function (originLat, originLng, destinationLat, destinationLng, pickupZipCode, dropOffZipCode, serviceType, vehicleTypeId, date, account) {
            
            var coordinates = {
                originLat: originLat,
                originLng: originLng,
                destinationLat: destinationLat,
                destinationLng: destinationLng,
                serviceType: serviceType,
                vehicleTypeId: vehicleTypeId,
                date: date
            }, tarifMode = TaxiHail.parameters.directionTarifMode, needAValidTarif = TaxiHail.parameters.directionNeedAValidTarif, fmt = 'json';

            function getDirectionInfoEvent() {
                var directionInfoDefer = $.Deferred();
                var tripDurationInSeconds = null;

                if (tarifMode != 'AppTarif') {

                    $.ajax({
                        url: 'api/directions/',
                        data: coordinates,
                        dataType: fmt,
                        success: function (result,status) {
                            tripDurationInSeconds = result.tripDurationInSeconds;
                        },
                        async: false
                    });

                    $.get('api/ibsfare?PickupLatitude={0}&PickupLongitude={1}&DropoffLatitude={2}&DropoffLongitude={3}&PickupZipCode={4}&DropoffZipCode={5}&AccountNumber={6}&CustomerNumber={7}&TripDurationInSeconds={8}&VehicleType={9}&ServiceType={10}'.format(
                            coordinates.originLat, coordinates.originLng, coordinates.destinationLat, coordinates.destinationLng, pickupZipCode, dropOffZipCode,
                            (account != null)
                                ? account
                                : '',
                            0,
                            (tripDurationInSeconds != null)
                                ? tripDurationInSeconds
                                : '',
                            vehicleTypeId,
                            serviceType
                            ),
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