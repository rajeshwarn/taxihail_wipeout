(function () {

    TaxiHail.directionInfo = _.extend(Backbone.Events, {
        getInfo: function (originLat, originLng, destLat, destLng) {
            return $.get('api/directions/', {
                OriginLat: originLat,
                OriginLng: originLng,
                DestinationLat: destLat,
                DestinationLng: destLng
            },function(){}, 'json')
            .done(function(result){
                result.callForPrice = (result.price > 100);
            });
        }
    });
}());