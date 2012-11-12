(function () {

    TaxiHail.directionInfo = _.extend({}, Backbone.Events, {
        getInfo: function (originLat, originLng, destinationLat, destinationLng, date) {
            return $.get('api/directions/', {
                originLat: originLat,
                originLng: originLng,
                destinationLat: destinationLat,
                destinationLng: destinationLng,
                date: date
            },function(){}, 'json')
            .done(function(result){
                result.callForPrice = (result.price > 100);
            });
        }
    });
}());