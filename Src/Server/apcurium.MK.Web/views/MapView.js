(function () {
    TaxiHail.MapView = Backbone.View.extend({
        
        initialize : function () {
            _.bindAll(this, "geolocdone", "geoloc");
            this.streetZoomLevel = 17;
            this.cityZoomLevel = 12;
            var self = this;                      
                this.interval = window.setInterval(function () {                    
                    self.refresh();
            }, 5000);

        },
        
        refresh: function () {
            this.availableVehicles = new TaxiHail.AvailableVehicleCollection([], { position: this._pickupPin.position });            
            var self = this;
            this.availableVehicles.fetch({
                success: function (response) {
                    self.availableVehicles = response;
                    self.updateAvailableVehiclesPosition();                    
                }
            });                 
        },

        setModel: function(model, centerMapOnAddressChange) {
            if(this.model) {
                this.model.off(null, null, this);
            }
            this.model = model;

            this.model.on('change:pickupAddress', function (model, value) {
                this.updatePickup();
                if(model.isValidAddress('pickupAddress')) {
                    var location = new google.maps.LatLng(value.latitude, value.longitude);
                    if (centerMapOnAddressChange) {
                        this.zoomMap(this.streetZoomLevel);
                        this.centerMap(location);
                    }
                }
            }, this);

            this.model.on('change:dropOffAddress', function (model, value) {
                this.updateDropOff();
                if(model.isValidAddress('dropOffAddress')) {
                    var location = new google.maps.LatLng(value.latitude, value.longitude);
                    if(centerMapOnAddressChange) this.centerMap(location);
                }
            }, this);
            

            this.model.on('change:isPickupActive change:isDropOffActive', function (model, value) {
                this._target.set('visible',  this.model.get('isPickupActive') || this.model.get('isDropOffActive'));
            }, this);

            this.model.getStatus().on('change:vehicleLatitude change:vehicleLongitude', (function(self) {
                var isAssigned = false;
                return _.bind(function(model) {
                    this.updateVehiclePosition(model);
                    if(!isAssigned && model.get('vehicleLatitude') && model.get('vehicleLongitude') )
                    {
                        // Center the map on the taxi the first time we have coordinates
                        this._bounds = new google.maps.LatLngBounds();
                        isAssigned = true;
                        this.centerMap(new google.maps.LatLng(model.get('vehicleLatitude'), model.get('vehicleLongitude')));
                        self.centerMapAroundVehicleAndPickup();
                    }
                }, self);
            }(this)));

            this.updatePickup();
            this.updateDropOff();
            this.updateVehiclePosition();
            this._target.set('visible',  this.model.get('isPickupActive') || this.model.get('isDropOffActive'));
            
        },
           
        render: function() {

            //default lat and long are defined in the default.aspx
            var mapOptions = {
                zoom: 12,
                center: new google.maps.LatLng(TaxiHail.parameters.defaultLatitude, TaxiHail.parameters.defaultLongitude),
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                overviewMapControl: false,
                panControl: false,
                rotateControl: false,
                streetViewControl: false,
                scaleControl: false,
                zoomControlOptions: {
                    position: google.maps.ControlPosition.LEFT_CENTER
                }
            };

            this._map = new google.maps.Map(this.el, mapOptions);
            google.maps.event.addListener(this._map, 'dragend', this.geoloc);
            
            this._vehicleMarker = new google.maps.Marker({
                position: this._map.getCenter(),
                map: this._map,
                icon: new google.maps.MarkerImage('assets/img/spacer.png', new google.maps.Size(1,1)),
                visible: false
            });
            var label = new TaxiLabel({
               map: this._map
            });
            label.bindTo('position', this._vehicleMarker, 'position');
            label.bindTo('text', this._vehicleMarker, 'text');
            label.bindTo('visible', this._vehicleMarker, 'visible');
            
            this._availableVehiclePins = {};

            this._bounds = new google.maps.LatLngBounds();

            this._pickupPin = new google.maps.Marker({
                position: this._map.getCenter(),
                zIndex: 99999,
                map: this._map,
                icon: 'assets/img/pin_green.png',
                visible: false
            });

            this._dropOffPin = new google.maps.Marker({
                position: this._map.getCenter(),
                map: this._map,
                icon: 'assets/img/pin_red.png',
                visible: false
            });

            var target = this._target = new Target({
                position: this._map.getCenter(),
                map: this._map,
                visible: false
            });


            // rename, remove under
            var $container = $(target.getMap().getDiv());

            this._mapSize = new google.maps.Point($container.width(), $container.height());

            this._mapSizeWithPadding = new google.maps.Point($container.width() * 3 / 4, $container.height() * 2 / 3);

            var onmapchanged = function () {
                var $_container = $(target.getMap().getDiv()),
                    x = $_container.width() / 2,
                    y = $_container.height() * 3 / 4,
                    projection = target.getProjection(),
                    position = projection.fromContainerPixelToLatLng(new google.maps.Point(x, y));
                target.set('position', position);
            };

            google.maps.event.addListener(this._map, 'bounds_changed', onmapchanged);
            google.maps.event.addListener(this._map, 'drag', onmapchanged);
            
            return this;
        },
        
        goToPickup: function () {
            if (this._pickupPin) this.centerMap(this._pickupPin.getPosition());
        },
        
        zoomMap: function (zoomLevel) {
            
            if (this._map.getZoom() < zoomLevel) {
                this._map.setZoom(zoomLevel);
            }
        },

        centerMap: function (location) {
            
            var projection = this._target.getProjection();
            
            if (projection) {
                //map ready center now
                this.centerFromProjection(projection, location);
                
            } else {
                //will center when the map is loaded
                var centerAfterLoad = _.bind(function () {
                    projection = this._target.getProjection();
                    this.centerFromProjection(projection, location);
                }, this);
                
                google.maps.event.addListenerOnce(this._map, 'tilesloaded', centerAfterLoad);
            }
        },
        
        centerFromProjection: function (projection, location) {
            var point = projection.fromLatLngToDivPixel(location);
            var center = new google.maps.Point(point.x, point.y - $(this._map.getDiv()).height() / 4);
            this._map.setCenter(projection.fromDivPixelToLatLng(center));
        },

        updateVehiclePosition: function(orderStatus) {
            var hasVehicle = orderStatus && orderStatus.hasVehicle();
            
            if (!hasVehicle) {
                this._vehicleMarker.setVisible(false);
            } else {
                this._vehicleMarker.setPosition(new google.maps.LatLng(orderStatus.get('vehicleLatitude'), orderStatus.get('vehicleLongitude')));
                this._vehicleMarker.setVisible(true);
                this._vehicleMarker.set('text', orderStatus.get('vehicleNumber'));
                this.centerMapAroundVehicleAndPickup();                
            }
        },

        centerMapAroundVehicleAndPickup: function () {
            var projection = this._target.getProjection();
            var bounds = new google.maps.LatLngBounds();
            bounds.extend(this._pickupPin.position);
            bounds.extend(this._vehicleMarker.position);
            this._map.fitBounds(bounds);
            
            var mainDiv = $('#main');
            var pickupPnt = projection.fromLatLngToContainerPixel(this._pickupPin.position);
            var vehiclePnt = projection.fromLatLngToContainerPixel(this._vehicleMarker.position);
            var mainDivBottom = $(mainDiv).offset().top + $(mainDiv).height();
            var mainDivLeft = $(mainDiv).offset().left;
            var mainDivRight = $(mainDiv).offset().left + $(mainDiv).width();
            var diffWithPickupY = pickupPnt.y - mainDivBottom;
            var diffWithVehicleY = vehiclePnt.y - mainDivBottom;
            var maxDiff = Math.max(diffWithPickupY, diffWithVehicleY);
            var vehicleInside = (vehiclePnt.x > mainDivLeft) && (vehiclePnt.x < mainDivRight) && (vehiclePnt.y < mainDivBottom);
            var pickupInside = (pickupPnt.x > mainDivLeft) && (pickupPnt.x < mainDivRight) && (pickupPnt.y < mainDivBottom);
            var anyInside = vehicleInside || pickupInside;

            if (anyInside) {
                var offsetY = new google.maps.Point(0, maxDiff);
                var extendByY = projection.fromContainerPixelToLatLng(offsetY);
                bounds.extend(bounds.getNorthEast());
                bounds.extend(extendByY);
                this._map.fitBounds(bounds);
            }
        },


        testic: function()
        {

        },

        updateAvailableVehiclesPosition: function () {

            // TODO: Used underscore lib to proceed in MapView, should use a view inside AvailableVehicleCollection if it's possible to avoid this dynamic/not managed marker approach (new marker etc)

            // Get vehicle backbone models as simple objects for underscore query purposes
            var _vehicles = _.map(this.availableVehicles.models, function (e) { return ({ vehicleNumber: e.vehicleNumber, latitude: e.latitude, longitude: e.longitude }) });

            // Get all existing available vehicle pin ID to manage them
            var _pins = _.map(this._availableVehiclePins, function (e) { return (e.metadata) });

            var self = this;

            _.each(_vehicles, function (_vehicle) {
                if (self._availableVehiclePins.hasOwnProperty(_vehicle.vehicleNumber)==false) {

                    // Add a new marker on the map
                    self._availableVehiclePins[_vehicle.vehicleNumber] = new google.maps.Marker({
                        position: new google.maps.LatLng(_vehicle.latitude, _vehicle.longitude),
                        map: self._map,
                        icon: 'assets/img/nearby_cab.png',
                        metadata: _vehicle.vehicleNumber
                    });
                    self.updatePickup();
                } else {

                    // Refresh existing marker on the map
                    var _car = self._availableVehiclePins[_vehicle.vehicleNumber];

                    // Verify that vehicle position changed to avoid flicker, or check if vehicle was previously unavailable
                    if (_vehicle.longitude.toFixed(4) != _car.position.lng().toFixed(4) || _vehicle.latitude.toFixed(4) != _car.position.lat().toFixed(4) || (_car.map != self._map)) {
                        _car.position = new google.maps.LatLng(_vehicle.latitude, _vehicle.longitude);
                        _car.setMap(self._map);
                        self.updatePickup();
                    }                    
                }
            });
            
            // Remove unused markers on the map
            _.each(_.difference(_pins, _.map(_vehicles, function (e) { return (e.vehicleNumber) })), function (_removedVehicle) {
                self._availableVehiclePins[_removedVehicle].setMap(null);
            });
        },
        
        updatePickup: function() {
            if(this._pickupPin) {
                this._pickupPin.setVisible(this.model.isValidAddress('pickupAddress'));
                if(this.model.isValidAddress('pickupAddress')) {
                    this._pickupPin.setPosition(new google.maps.LatLng(this.model.get('pickupAddress').latitude, this.model.get('pickupAddress').longitude));
                }
            }
        },

        updateDropOff: function() {
            if(this._dropOffPin) {
                this._dropOffPin.setVisible(this.model.isValidAddress('dropOffAddress'));
                if(this.model.isValidAddress('dropOffAddress')) {
                    this._dropOffPin.setPosition(new google.maps.LatLng(this.model.get('dropOffAddress').latitude, this.model.get('dropOffAddress').longitude));
                }
            }
        },
        
        geolocdone : function (result) {
            if (result && result.length) {
                if (this.model.get('isPickupActive')) {
                    this.model.set('pickupAddress', result[0]);
                } else {
                    this.model.set('dropOffAddress', result[0]);
                }
            }
        },

        geoloc: function () {
            var position = this._target.get('position');
            if (this.model.get('isPickupActive') || this.model.get('isDropOffActive')) {
                if (this.model.get('isPickupActive') && this._pickupPin) {
                    this._pickupPin.setPosition(position);
                    this._pickupPin.setVisible(true);
                } else if (this.model.get('isDropOffActive') && this._dropOffPin) {
                    this._dropOffPin.setPosition(position);
                    this._dropOffPin.setVisible(true);
                }

                TaxiHail.geocoder.geocode(position.lat(), position.lng())
                    .done(this.geolocdone);
            }
        },
        readjustZoomAfterFitBounds: function (bounds, mapDim) {

            var WORLD_DIM = { height: 256, width: 256 };
            var ZOOM_MAX = 21;

            function latRad(lat) {
                var sin = Math.sin(lat * Math.PI / 180);
                var radX2 = Math.log((1 + sin) / (1 - sin)) / 2;
                return Math.max(Math.min(radX2, Math.PI), -Math.PI) / 2;
            }

            function zoom(mapPx, worldPx, fraction) {
                return Math.floor(Math.log(mapPx / worldPx / fraction) / Math.LN2);
            }
                
            var ne = bounds.getNorthEast();
            var sw = bounds.getSouthWest();

            var latFraction = (latRad(ne.lat()) - latRad(sw.lat())) / Math.PI;

            var lngDiff = ne.lng() - sw.lng();
            var lngFraction = ((lngDiff < 0) ? (lngDiff + 360) : lngDiff) / 360;

            var latZoom = zoom(mapDim.height, WORLD_DIM.height, latFraction);
            var lngZoom = zoom(mapDim.width, WORLD_DIM.width, lngFraction);
            return Math.min(latZoom, lngZoom, ZOOM_MAX);
        }
    });

    var TaxiLabel = function(opt_options) {
        // Initialization
        this.setValues(opt_options);

        // TaxiLabel specific
        var span = this.span_ = document.createElement('span');
        span.style.cssText = 'position: relative; display:block; width:100%; top: 13px; white-space: nowrap; font-size: 24px; font-weight: bold; color: white; text-align: center;';
        var marker = this.marker_ = document.createElement('div');
        marker.style.cssText = 'position: relative; width: 106px; height:80px; left: -53px; top: -70px; background: url(assets/img/taxi_label.png);';
        marker.appendChild(span);
        var div = this.div_ = document.createElement('div');
        div.appendChild(marker);
        div.style.cssText = 'position: absolute; display: none;';
    };

    _.extend(TaxiLabel.prototype, new google.maps.OverlayView(), {
        onAdd: function() {
            var pane = this.getPanes().overlayLayer;
            pane.style.zIndex = 121;
             pane.appendChild(this.div_);

             // Ensures the label is redrawn if the text or position is changed.
             var me = this;
             this.listeners_ = [
               google.maps.event.addListener(this, 'position_changed',
                   function() { me.draw(); }),
               google.maps.event.addListener(this, 'text_changed',
                   function() { me.draw(); }),
               google.maps.event.addListener(this, 'visible_changed',
                   function() { me.draw(); })
             ];
         },
         onRemove: function() {
            this.div_.parentNode.removeChild(this.div_);

            // TaxiLabel is removed from the map, stop updating its position/text.
            for (var i = 0, I = this.listeners_.length; i < I; ++i) {
               google.maps.event.removeListener(this.listeners_[i]);
            }
        },
        draw: function() {
            if(!this.get('visible')) {
                this.div_.style.display = 'none';
                return;
            }

            var projection = this.getProjection();
            var position = projection.fromLatLngToDivPixel(this.get('position'));

            var div = this.div_;
            div.style.left = position.x + 'px';
            div.style.top = position.y + 'px';
            div.style.display = 'block';

            this.span_.innerHTML = this.get('text') || '';
         }
    });

    var Target = function(options) {
        // Initialization
        this.setValues(options);

        var div = this.div_ = document.createElement('div');
        div.style.cssText = 'width: 25px; height: 25px; background: url(assets/img/target.png) top left no-repeat;position: absolute; display: none';
    };

    _.extend(Target.prototype, new google.maps.OverlayView(), {
        onAdd: function() {
            var pane = this.getPanes().overlayLayer;
            pane.style.zIndex = 121;
            pane.appendChild(this.div_);

             var me = this;
             this.listeners_ = [
             google.maps.event.addListener(this, 'position_changed',
                   function() { me.draw(); }),
               google.maps.event.addListener(this, 'visible_changed',
                   function() { me.draw(); })
             ];
         },
         onRemove: function() {
            this.div_.parentNode.removeChild(this.div_);

            for (var i = 0, I = this.listeners_.length; i < I; ++i) {
               google.maps.event.removeListener(this.listeners_[i]);
            }
        },
        draw: function() {
            if(!this.get('visible')) {
                this.div_.style.display = 'none';
                return;
            }

            var projection = this.getProjection();
            var position = projection.fromLatLngToDivPixel(this.get('position'));

            var div = this.div_;
            div.style.left = (position.x - 12) + 'px';
            div.style.top =  (position.y - 12) + 'px';
            div.style.display = 'block';
         }
    });

}());