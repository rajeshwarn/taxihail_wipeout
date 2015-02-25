(function() {
    TaxiHail.AvailableVehicleCollection = Backbone.Collection.extend({
        model: TaxiHail.AvailableVehicle,
        initialize: function (models, options) {
            this.position = options.position;
            this.market = options.market;
        },
        url: function () {
            var queryString = '/vehicles/' + '?latitude=' + this.position.lat() + '&longitude=' + this.position.lng();
            if (this.market) {
                queryString += "&market=" + this.market;
            }
            queryString += "&format=json";

            return TaxiHail.parameters.apiRoot + queryString;
        },
        parse: function (response) {
            var lst = [];
            _.each(response, function (model) {
                var vehicle = new TaxiHail.AvailableVehicle([],  { latitude: model.latitude, longitude: model.longitude, vehicleNumber: model.vehicleNumber });               
                lst.push(vehicle);
            });                  
            return lst;
        }
    });    
}());