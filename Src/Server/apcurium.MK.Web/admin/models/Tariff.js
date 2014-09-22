(function(){

    var Tariff = TaxiHail.Tariff = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/tariffs',
        defaults: function(){
            var today = new Date();
            return {
                name: '',
                minimumRate: 0,
                flatRate: 0,
                kilometricRate: 0,
                perMinuteRate: 0,
                marginOfError: 20,
                kilometerIncluded :0,
                startTime: TaxiHail.date.toISO8601(new Date(today.getYear(), today.getMonth(), today.getDate())),
                endTime: TaxiHail.date.toISO8601(new Date(today.getYear(), today.getMonth(), today.getDate() + 1)),
                daysOfTheWeek: 0,
                vehicleTypeId: null
            };
        }
    }, {
        type: {
            'default': 0,
            recurring: 1,
            day: 2,
            vehicleDefault: 3
        }
    });

    Tariff.prototype.defaults.type = Tariff.type.recurring;

}());