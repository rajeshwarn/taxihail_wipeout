(function () {

    TaxiHail.Order = Backbone.Model.extend({

        url: 'api/account/orders',

        initialize: function(attributes, options) {
            
        },

        validate: function(attrs) {

            if(attrs.pickupAddress
                && attrs.pickupAddress.fullAddress
                && attrs.pickupAddress.latitude
                && attrs.pickupAddress.longitude)
                {
                    // We have a pickup address
                    // Order is valid
                    return;
                } 

            // Missing pickup address
            // Order is invalid
            return "invalid";

        }
        
    });

}());