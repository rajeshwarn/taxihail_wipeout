(function () {

    TaxiHail.math = {

        initialize: function () {
           
        },

        distanceBeetweenTwoLatLgt: function (lat1, lon1, lat2, lon2) {

            //full accuracy formula
            var R = 6371; // km
            var d = Math.acos(Math.sin(this.toRad(lat1)) * Math.sin(this.toRad(lat2)) +
                              Math.cos(this.toRad(lat1)) * Math.cos(this.toRad(lat2)) *
                              Math.cos(this.toRad(lon2) - this.toRad(lon1)) * R);
            //less accuracy but 6 * more performant
           /* var R = 6371; // km
            var x = (lon2 - lon1) * Math.cos((lat1 + lat2) / 2);
            var y = (lat2 - lat1);
            var d = Math.sqrt(x * x + y * y) * R;*/
            return d;
        },
        
        toRad:function(number) {
            return number * Math.PI / 180;
        }
        
    };
}());