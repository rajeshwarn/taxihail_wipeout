(function () {

    TaxiHail.RideRatingCollection = Backbone.Collection.extend({
        model: TaxiHail.RideRatings,
        url: TaxiHail.parameters.apiRoot + '/ratingtypes/'
    });
}());