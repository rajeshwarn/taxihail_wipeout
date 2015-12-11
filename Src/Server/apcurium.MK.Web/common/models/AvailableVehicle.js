(function () {
    TaxiHail.AvailableVehicle = Backbone.Model.extend({
        defaults: {
            latitude: "50",
            longitude: "-45",
            vehicleName: 0
        },
        initialize: function (attributes, options) {
            this.latitude = options.latitude;
            this.longitude = options.longitude;
            this.vehicleName = options.vehicleName;
        }
    });

    var getValue = function (object, prop) {
        if (!(object && object[prop])) return null;
        return _.isFunction(object[prop]) ? object[prop]() : object[prop];
    };

}());