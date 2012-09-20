(function () {

    TaxiHail.MapView = Backbone.View.extend({
        
        initialize: function () {
        },

        render: function() {

            var mapOptions = {
                zoom: 8,
                center: new google.maps.LatLng(-34.397, 150.644),
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            
            this._map = new google.maps.Map(this.el, mapOptions);

            return this;
        }
        
    });

}());