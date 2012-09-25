(function () {

    TaxiHail.Order = Backbone.Model.extend({

        url: 'api/account/orders',

        validate: function(attrs) {

            if(this._addressIsValid(attrs.pickupAddress))
            {
                // We have a pickup address
                // Order is valid
                return;
            } 

            // Missing pickup address
            // Order is invalid
            return "invalid";

        },

        _addressIsValid: function(address){

            return address
                && address.fullAddress
                && address.latitude
                && address.longitude;

        }

    });

}());