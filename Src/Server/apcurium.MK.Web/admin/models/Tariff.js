(function(){

    var Tariff = TaxiHail.Tariff = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/tariffs',
        defaults: function(){
            var today = new Date();
            return { name: '',
                flatRate: 0,
                pricePerPassenger: 0,
                distanceMultiplicator: 0,
                timeAdjustmentFactor: 20,
                startTime: TaxiHail.date.toISO8601(new Date(today.getYear(), today.getMonth(), today.getDate())),
                endTime: TaxiHail.date.toISO8601(new Date(today.getYear(), today.getMonth(), today.getDate() + 1)),
                daysOfTheWeek: 0
            };
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