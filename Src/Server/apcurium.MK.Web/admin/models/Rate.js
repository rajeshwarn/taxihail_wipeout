(function(){

    TaxiHail.Rate = Backbone.Model.extend({
        defaults: {
            type: 'recurring',
            name: '',
            flatRate: 0,
            pricePerPassenger: 0,
            distanceMultiplicator: 1,
            timeAdjustmentFactor: 1,
            startTime: null,
            endTime: null,
            daysOfTheWeek: 0
        }
    });

}());