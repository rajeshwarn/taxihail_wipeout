(function () {

    String.prototype.format = String.prototype.format = function () {
        var s = this,
            i = arguments.length;

        while (i--) {
            s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
        }
        return s;
    };

    _.extend(TaxiHail, {

        getClientLanguage: function () {
            return (navigator.language) ? navigator.language.split('-')[0] : navigator.userLanguage;
        },

        formatEta: function (duration, formattedDistance) {
            var durationUnit = duration <= 1 ? TaxiHail.localize("EtaDurationUnit") : TaxiHail.localize("EtaDurationUnitPlural");
            return TaxiHail.localize("Eta").format(formattedDistance, duration, durationUnit);
        },

        formatAssignedEta: function (duration, formattedDistance) {
            var durationUnit = duration <= 1 ? TaxiHail.localize("EtaDurationUnit") : TaxiHail.localize("EtaDurationUnitPlural");
            return TaxiHail.localize("AssignedEta").format(formattedDistance, duration, durationUnit);
        },
        
        localize: function (resourceName, resourceSet) {
            var resource = '';
            var appKey = TaxiHail.parameters.applicationKey;
            var lang = TaxiHail.getClientLanguage();
            
            var localizedCompanyResourceSet = appKey + "-" + lang;
            if (!_.isUndefined(TaxiHail.resources[localizedCompanyResourceSet])) {
                resource = TaxiHail.resources[localizedCompanyResourceSet][resourceName];
            }

            if (!resource && resourceSet) {
                var localizedResourceSet = resourceSet + "-" + lang;

                if (!_.isUndefined(TaxiHail.resources[localizedResourceSet])) {
                    resource = TaxiHail.resources[localizedResourceSet][resourceName];
                } else if (!_.isUndefined(TaxiHail.resources[resourceSet])) {
                    resource = TaxiHail.resources[resourceSet][resourceName];
                }
            }

            // Check for localized Global resource
            var localizedGlobalResourceSet = "Global-" + lang;
            if (!resource && !_.isUndefined(TaxiHail.resources[localizedGlobalResourceSet])) {
                resource = TaxiHail.resources[localizedGlobalResourceSet][resourceName];
            }

            return resource || TaxiHail.resources.Global[resourceName] || resourceName;
        },

        addResourceSet: function (name, resourceSet) {
            TaxiHail.resources[name] = resourceSet;
        },

        postpone: function (func, context) {
            return _.debounce(_.bind(func, context), 800);
        },
        
        confirm: function (options) {

            var defaults = {
                title: '[Title]',
                message: '[Message]',
                confirmButton: TaxiHail.localize('modal.default.confirmButton'),
                cancelButton: TaxiHail.localize('modal.default.cancelButton')
            }, events = _.extend({}, Backbone.Events);

            options = _.extend(defaults, options);

            var view = new TaxiHail.BootstrapConfirmationView(options);

            $('.modal-zone').html(view.render().el);

            view.show();

            view.on('ok', _.once(function(arg1, arg2, arg3){
                events.trigger('ok', view);
            }));
            view.on('cancel', _.once(function(){
                events.trigger('cancel', view);
            }));
            return events;
        },

        showSpinner: function(container) {
            var spinner = new Spinner({
                lines: 11, // The number of lines to draw
                length: 3, // The length of each line
                width: 3, // The line thickness
                radius: 6, // The radius of the inner circle
                corners: 1, // Corner roundness (0..1)
                rotate: 0, // The rotation offset
                speed: 1, // Rounds per second
                trail: 60, // Afterglow percentage
                shadow: false, // Whether to render a shadow
                hwaccel: false, // Whether to use hardware acceleration
                className: 'spinner-address', // The CSS class to assign to the spinner
                zIndex: 2e9 // The z-index (defaults to 2000000000)
            }).spin();
            
            $(container).html(spinner.el);
        },

        date: {
            toISO8601: function(date) {
                var year = date.getFullYear(),
                    month = date.getMonth() + 1,
                    day = date.getDate(),
                    hour = date.getHours(),
                    minute = date.getMinutes(),
                    second = date.getSeconds();

                month = month < 10 ? '0' + month : month;
                day = day < 10 ? '0' + day : day;
                hour = hour < 10 ? '0' + hour : hour;
                minute = minute < 10 ? '0' + minute : minute;
                second = second < 10 ? '0' + second : second;

                return year + '-' + month + '-' + day + 'T' + hour + ':' + minute + ':' + second;
            },
            
            ISO8601toJs: function (date) {
                if (_.isString(date) && date.indexOf('-') && date.indexOf('T') && date.indexOf(':')) {
                    // We assume that we have a date in the format : yyyy-mm-ddThh:mm:ss
                    var parts = date.split('T');
                    var dateParts = parts[0].split('-');
                    if (dateParts.length === 3) {
                        var year = parseInt(dateParts[0], 10),
                            month = parseInt(dateParts[1], 10) - 1,
                            day = parseInt(dateParts[2], 10);
                        var timeParts = parts[1].split(':');
                        if (timeParts.length === 3) {
                            var hour = parseInt(timeParts[0], 10),
                                minute = parseInt(timeParts[1], 10) ,
                                sec = parseInt(timeParts[2], 10);

                            date = new Date(year, month, day, hour, minute, sec);
                            return date;
                        }
                    }
                }
                return '';
            },
            niceDate: function(date) {
                if(_.isString(date) && date.indexOf('-') && date.indexOf('T') && date.indexOf(':')) {
                    // We assume that we have a date in the format : yyyy-mm-ddThh:mm:ss
                    var days = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];
                    var months = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
                    var parts = date.split('T');
                    var dateParts = parts[0].split('-');
                    if(dateParts.length === 3) {
                        var year = parseInt(dateParts[0], 10),
                            month = parseInt(dateParts[1], 10) -1,
                            day = parseInt(dateParts[2], 10);

                        date = new Date(year, month, day, 0, 0, 0);

                        var dayOfTheWeek = date.getDay();

                        // Format: Monday, August 17
                        return new Handlebars.SafeString(days[dayOfTheWeek] + ',\u00a0 ' + months[month] + ' ' + day);
                    }
                }
                // not needed yet
                return '';
            },
            
            numericDate: function (date) {
                if (_.isString(date) && date.indexOf('-') && date.indexOf('T') && date.indexOf(':')) {
                    // We assume that we have a date in the format : yyyy-mm-ddThh:mm:ss
                    var parts = date.split('T');
                    var dateParts = parts[0].split('-');
                    return  new Handlebars.SafeString(dateParts[0]+'/'+dateParts[1]+'/'+dateParts[2]);
                }
                // not needed yet
                return '';
            },
            
            niceShortDate: function (date) {
                if (_.isString(date) && date.indexOf('-') && date.indexOf('T') && date.indexOf(':')) {
                    // We assume that we have a date in the format : yyyy-mm-ddThh:mm:ss
                    var days = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
                    var months = ["Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sep", "Oct", "Nov", "Dec"];
                    var parts = date.split('T');
                    var dateParts = parts[0].split('-');
                    if (dateParts.length === 3) {
                        var year = parseInt(dateParts[0], 10),
                            month = parseInt(dateParts[1], 10) - 1,
                            day = parseInt(dateParts[2], 10);

                        date = new Date(year, month, day, 0, 0, 0);

                        var dayOfTheWeek = date.getDay();

                        // Format: Monday, August 17
                        return new Handlebars.SafeString(days[dayOfTheWeek] + ',\u00a0 ' + months[month] + ' ' + day);
                    }
                }
                // not needed yet
                return '';
            },
            niceTime: function(date) {
                if(_.isString(date) && date.indexOf('-') && date.indexOf('T') && date.indexOf(':')) {
                    // We assume that we have a date in the format : yyyy-mm-ddThh:mm:ss
                    var parts = date.split('T'),
                        timeParts = parts[1].split(':'),
                        meridian = "AM";
                    if(timeParts.length >= 2) {
                        var hour = parseInt(timeParts[0], 10);
                        var minute = parseInt(timeParts[1], 10);

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

                        // Format: 2:35 PM
                        return new Handlebars.SafeString(hour + ":" + minute + "\u00a0" + meridian);
                    }
                }
                // not needed yet
                return '';
            }
        }

    });

    Handlebars.registerHelper('localize', function (resourceName) {
        return new Handlebars.SafeString(TaxiHail.localize(resourceName, this.resourceSet));
    });

    Handlebars.registerHelper('localizeBool', function (value, options) {
        if (value === true) return options.hash["true"];
        if (value === false) return options.hash["false"];

        if (value == "true") return options.hash["true"];
        if (value == "false") return options.hash["false"];
        return 'not a bool';
    });
    
    Handlebars.registerHelper('isBool', function (obj,options) {
        if (obj == "true" || obj == "false") {
            return options.fn(this);
        } else {
            return options.inverse(this);
        }
    });
    
    Handlebars.registerHelper('isNumber', function (n, options) {
        if (!isNaN(parseFloat(n)) && isFinite(n)) {
            return options.fn(this);
        } else {
            return options.inverse(this);
        }
    });
    
    Handlebars.registerHelper('invertedBool', function (obj) {
        if (obj === true) {
            return "false";
        }
        if (obj === false) {
            return "true";
        }


        if (obj == "true") {
            return "false";
        }
        if (obj == "false") {
            return "true";
        }
        
        return "param is not a bool";
        
    });

    Handlebars.registerHelper('ifCond', function (v1, v2, options) {
        if (v1 == v2) {
            return options.fn(this);
        } else {
            return options.inverse(this);
        }
    });

    Handlebars.registerHelper('ifNotCond', function (v1, v2, options) {
        if (v1 != v2) {
            return options.fn(this);
        } else {
            return options.inverse(this);
        }
    });

    Handlebars.registerHelper('niceShortDateAndTime', function (date) {
        // Format: Monday, August 17 at 2:35 PM
        return new Handlebars.SafeString(TaxiHail.date.niceShortDate(date) + ',\u00a0' + TaxiHail.date.niceTime(date));
    });
    
    Handlebars.registerHelper('niceDateAndTime', function (date) {
        // Format: Monday, August 17 at 2:35 PM
        return new Handlebars.SafeString(TaxiHail.date.niceDate(date) + '\u00a0at\u00a0' + TaxiHail.date.niceTime(date));
    });

    Handlebars.registerHelper('niceDate', function (date) {
        return new Handlebars.SafeString(TaxiHail.date.niceDate(date));
    });

    Handlebars.registerHelper('niceTime', function(date) {
        return new Handlebars.SafeString(TaxiHail.date.niceTime(date));
    });
    
    Handlebars.registerHelper('numericDate', function (date) {
        return new Handlebars.SafeString(TaxiHail.date.numericDate(date));
    });

    // Select a dropdown box item.
    // Usage: wrap <option></option> tags with {{#select value}}{{/select}}
    Handlebars.registerHelper('select', function (value, options) {
        var $el = $('<select />').html(options.fn(this));
        $el.find('[value=' + value + ']').attr({ 'selected': 'selected' });
        return $el.html();
    });

    $.validator.addMethod("checkboxesNotAllChecked", function (value, elem, param) {
        if ($(elem).closest("div").find(":checked").length == param.options.length) {
            return false;
        } else {
            return true;
        }
    }, TaxiHail.localize("You cannot exclude all options from a list."));

    // this prevents the hidden radio buttons from not being validated
    $.validator.setDefaults({
        ignore: []
    });
}());