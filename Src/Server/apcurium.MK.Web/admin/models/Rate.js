(function(){

    var Rate = TaxiHail.Rate = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/rates',
        defaults: {
            name: '',
            flatRate: 0,
            pricePerPassenger: 0,
            distanceMultiplicator: 0,
            timeAdjustmentFactor: 10,
            startTime: null,
            endTime: null,
            daysOfTheWeek: 0
        }
    }, {
        type: {
            recurring: 1,
            day: 2
        }
    });

    Rate.prototype.defaults.type = Rate.type.recurring;

}());