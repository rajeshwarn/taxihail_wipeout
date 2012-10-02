(function () {

    _.extend(TaxiHail, {

        localize: function (resourceName, resourceSet) {
            var resource = '';
            if (resourceSet && !_.isUndefined(TaxiHail.resources[resourceSet])) {
                resource = TaxiHail.resources[resourceSet][resourceName];
            }
            return resource || TaxiHail.resources.Global[resourceName] || '[' + resourceName + ']';
        },

        addResourceSet: function (name, resourceSet) {
            TaxiHail.resources[name] = resourceSet;
        },

        postpone: function (func, context) {
            return _.debounce(_.bind(func, context), 800);
        },

    });

    Handlebars.registerHelper('localize', function (resourceName) {
        return new Handlebars.SafeString(TaxiHail.localize(resourceName, this.resourceSet));
    });

    Handlebars.registerHelper('niceDate', function (date) {
        if(_.isString(date) && date.indexOf('-') && date.indexOf('T') && date.indexOf(':'))
        {
            // Wild assumption that we have a date in the format : yyyy-mm-ddThh:mm:ss
            var parts = date.split('T');
            var dateParts = parts[0].split('-');
            var timeParts = parts[1].split(':');
            if(dateParts.length === 3 && timeParts.length >= 2) {
                var year = dateParts[0];
                var month = dateParts[1];
                var day = dateParts[2];
                var hour = timeParts[0];
                var minute = timeParts[1];

                var meridian = "AM";
                if (hour === 0) {
                    hour = 12;
                } else if (hour >= 12) {
                    if (hour > 12) {
                        hour = hour - 12;
                    }
                    meridian = "PM";
                } else {
                   meridian = "AM";
                }

                return new Handlebars.SafeString(month + '/' + day + '/' + year + '\u00A0@\u00A0' + hour + ":" + minute + "\u00A0" + meridian);
            }
        }
        // not needed yet
        return '';
    });


}());