(function() {
    TaxiHail.AvailableVehicleCollection = Backbone.Collection.extend({
        model: TaxiHail.AvailableVehicle,
        initialize: function (models, options) {
            this.position = options.position;                                 
        },
        url: function () {
            var market = TaxiHail.parameters.market;
            return TaxiHail.parameters.apiRoot + '/vehicles/' + '?latitude=' + this.position.lat() + '&longitude=' + this.position.lng() + "&market=" + (market != null ? market : "") + "&format=json"; // TODO: Remove debug
        },
        parse: function (response) {
            var collection = new TaxiHail.AvailableVehicleCollection([], this.position);            
            var _lst = [];
            _.each(response, function (model, index) {
                var vehicle = new TaxiHail.AvailableVehicle([],  { latitude: model.latitude, longitude: model.longitude, vehicleNumber: model.vehicleNumber });               
                _lst.push(vehicle);
            });                  
            collection = _lst;
            return collection;
        }
    });    
}());