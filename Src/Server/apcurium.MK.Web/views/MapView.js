(function () {

    TaxiHail.MapView = Backbone.View.extend({
        
        initialize: function () {

            this.$el.addClass('map-canvas');
            
            var mapOptions = {
                zoom: 8,
                center: new google.maps.LatLng(-34.397, 150.644),
                mapTypeId: google.maps.MapTypeId.ROADMAP
            };
            this._map = new google.maps.Map(this.el, mapOptions);
        },
        
    });

}());