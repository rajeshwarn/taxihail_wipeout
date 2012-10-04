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
        
        confirm: function (title, message, okfunc) {
            var view = new TaxiHail.BootstrapConfirmationView({
                model: new Backbone.Model({
                    title: title,
                    message: message
                })
            });

            $('.modal-zone').html(view.render().el);

            view.show();
            view.on('ok', _.once(okfunc));
        }

    });

    Handlebars.registerHelper('localize', function (resourceName) {
        return new Handlebars.SafeString(TaxiHail.localize(resourceName, this.resourceSet));
    });

    Handlebars.registerHelper('ifCond', function (v1, v2, options) {
        if (v1 == v2) {
            return options.fn(this);
        } else {
            return options.inverse(this);
        }
    });

    Handlebars.registerHelper('niceDate', function (date) {
        if(_.isString(date) && date.indexOf('-') && date.indexOf('T') && date.indexOf(':'))
        {
            // Wild assumption that we have a date in the format : yyyy-mm-ddThh:mm:ss
            var days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
            var months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
            var parts = date.split('T');
            var dateParts = parts[0].split('-');
            var timeParts = parts[1].split(':');
            if(dateParts.length === 3 && timeParts.length >= 2) {
                var year = parseInt(dateParts[0], 10);
                var month = parseInt(dateParts[1], 10) -1;
                var day = parseInt(dateParts[2], 10);
                var hour = parseInt(timeParts[0], 10);
                var minute = parseInt(timeParts[1], 10);

                date = new Date(year, month, day, hour, minute, 0);

                var dayOfTheWeek = date.getDay();

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

                minute = minute < 10 ? '0' + minute : minute;

                // Format: Monday, August 17 at 2:35 PM
                return new Handlebars.SafeString(days[dayOfTheWeek] + ',\u00a0 ' + months[month] + ' ' + day + '\u00a0at\u00a0' + hour + ":" + minute + "\u00a0" + meridian);
            }
        }
        // not needed yet
        return '';
    });


}());