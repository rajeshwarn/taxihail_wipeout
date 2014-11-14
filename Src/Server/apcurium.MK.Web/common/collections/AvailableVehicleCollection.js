(function() {
    TaxiHail.AvailableVehicleCollection = Backbone.Collection.extend({
        model: TaxiHail.AvailableVehicle,
        initialize: function (models, options) {
            this.position = options.position;
            this.market = options.market;
        },
        url: function () {
            return TaxiHail.parameters.apiRoot + '/vehicles/' + '?latitude=' + this.position.lat() + '&longitude=' + this.position.lng() + "&market=" + this.market + "&format=json";
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