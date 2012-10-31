(function(){

    var Tariff = TaxiHail.Tariff = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/tariffs',
        defaults: {
            name: '',
            flatRate: 0,
            pricePerPassenger: 0,
            distanceMultiplicator: 0,
            timeAdjustmentFactor: 20,
            startTime: null,
            endTime: null,
            daysOfTheWeek: 0
        }
    }, {
        type: {
            'default': 0,
            recurring: 1,
            day: 2
        }
    });

    Tariff.prototype.defaults.type = Tariff.type.recurring;

}());