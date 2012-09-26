(function () {
    var offsetY = -200;
    TaxiHail.MapView = Backbone.View.extend({
        
        events: {
        },
        
        initialize : function () {
            _.bindAll(this, "geolocdone");
            _.bindAll(this, "geoloc");
        },
        
        setModel: function(model) {
            if(this.model) {
                this.model.off(null, null, this);
            }
            this.model = model;

            this.model.on('change:pickupAddress', function (model, value) {
                var location = new google.maps.LatLng(value.latitude, value.longitude);
                var loc = this.offsetX(location, 0, offsetY);
                if (this._pickupPin) {
                   // this.offsetCenter(location, 0, -200);

                    this._pickupPin.setPosition(location);

                } else {
                    this._pickupPin = this.addMarker(location, 'http://maps.google.com/mapfiles/ms/icons/green-dot.png');
                }
                
                this._map.setCenter(loc);
                //this._map.setCenter(location);
            }, this);

            this.model.on('change:dropOffAddress', function (model, value) {
                var location = new google.maps.LatLng(value.latitude, value.longitude);
                var loc = this.offsetX(location, 0, offsetY);
                if (this._dropOffPin) {
                    this._dropOffPin.setPosition(location);
                } else {
                    this._dropOffPin = this.addMarker(location, 'http://maps.google.com/mapfiles/ms/icons/red-dot.png');
                }
                this._map.setCenter(loc);
            }, this);
            

            offsetY = -(this.$el.height() / 4);
            var targetX = ( this.$el.width() / 2 ) -13;
            var targetY = ( this.$el.height() / 1.5) + 45 ;
            this.model.on('change:isPickupActive change:isDropOffActive', function (model, value) {
                if (model.get('isPickupActive') == true || model.get('isDropOffActive') == true) {
                    if ($("#target-icon").length==0) {
                        this.$el.append($('<img id="target-icon" src="themes/generic/img/target.png"  style=" width : 25px ; position : absolute; top :' + targetY + 'px ; left :' + targetX + 'px;">'));
                    }
                } else {
                    $("#target-icon").remove();
                }
            }, this);
            
        },
        
        

           
        render: function() {

            var mapOptions = {
                zoom: 12,
                center: new google.maps.LatLng(-34.397, 150.644),
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                zoomControlOptions: {
                    position: google.maps.ControlPosition.LEFT_CENTER,

                }

            };
            this._map = new google.maps.Map(this.el, mapOptions);
            google.maps.event.addListener(this._map, 'dragend', this.geoloc);

            
            

            return this;

        },
        
        geolocdone : function (result) {
            if (result.addresses && result.addresses.length) {
                if (this.model.get('isPickupActive')) {
                    this.model.set('pickupAddress', result.addresses[0]);
                } else {
                    this.model.set('dropOffAddress', result.addresses[0]);
                }
            }
        },
        
        geoloc: function () {
            if (this.model.get('isPickupActive') == true || this.model.get('isDropOffActive') == true) {
                if (this.model.get('isPickupActive')) {
                    if (this._pickupPin) {
                        var loc = this.offsetX(this._map.getCenter(), 0, -offsetY);
                        this._pickupPin.setPosition(loc);
                    } else {

                        this._pickupPin = this.addMarker(this._map.getCenter(), 'http://maps.google.com/mapfiles/ms/icons/green-dot.png');

                    }
                    //TaxiHail.geocoder.geocode(this.offsetX(this._map.getCenter(), 0, 200).Xa, this.offsetX(this._map.getCenter(), 0, -offsetY).Ya)
                   //     .done(this.geolocdone);
            } else {
                if (this._dropOffPin) {
                    //this._dropOffPin.setPosition(this._map.getCenter());
                    var loc = this.offsetX(this._map.getCenter(), 0, -offsetY);
                    this._dropOffPin.setPosition(loc);
                } else {
                    this._dropOffPin = this.addMarker(this._map.getCenter(), 'http://maps.google.com/mapfiles/ms/icons/red-dot.png');
                }

                    //TaxiHail.geocoder.geocode(this.offsetX(this._map.getCenter(), 0, -offsetY).Xa, this.offsetX(this._map.getCenter(), 0, -offsetY).Ya)
                    //        .done(this.geolocdone);
            }
            }
            
        },
        
        offsetX: function (latlng, offsetx, offsety) {


            var point1 = this._map.getProjection().fromLatLngToPoint(
        (latlng instanceof google.maps.LatLng) ? latlng : this._map.getCenter()
           );
            var point2 = new google.maps.Point(
                ((typeof (offsetx) == 'number' ? offsetx : 0) / Math.pow(2, this._map.getZoom())) || 0,
                ((typeof (offsety) == 'number' ? offsety : 0) / Math.pow(2, this._map.getZoom())) || 0
            );
            return this._map.getProjection().fromPointToLatLng(new google.maps.Point(
                point1.x - point2.x,
                point1.y + point2.y));
            
            

        },


        addMarker: function (location, iconImage) {
            var loc = this.offsetX(location, 0, -offsetY);
            return new google.maps.Marker({
                position: loc,
                map: this._map,
                icon: iconImage
            } );
        }

        
    });

}());