(function(){
    

    var View = TaxiHail.EditRateView = TaxiHail.TemplatedView.extend({

        render: function() {

            var daysOfTheWeek = this.model.get('daysOfTheWeek'),
                now = new Date(),
                today = new Date(now.getFullYear(), now.getMonth(), now.getDate()),
                data = this.model.toJSON();

            data.recurring = +this.model.get('type') === TaxiHail.Rate.type.recurring;

            // Determine if the checkbox for each days should be checked
            // Will produce un object like this
            // { checked0: 'checked', checked1: '' ... }
            _.extend(data, _(_.range(7)).reduce(function(memo, num) {
                var flagValue = 1 << num,
                    isChecked = (daysOfTheWeek & flagValue) === flagValue;

                memo['checked' + num] = isChecked ? 'checked' : '';
                return memo;
            }, {}));


            this.$el.html(this.renderTemplate(data));

            this.$('[data-role=timepicker]').timepicker({
                defaultTime: 'value'
            });
            this.$('[data-role=datepicker]').datepicker().datepicker('setValue', today);

            this.validate({
                rules: {
                    name: 'required',
                    flatRate: {
                        required: true,
                        min: 0
                    },
                    distanceMultiplicator: {
                        required: true,
                        min:0
                    },
                    timeAdjustmentFactor: {
                        required: true,
                        min:0
                    },
                    pricePerPassenger: {
                        required: true,
                        min: 0
                    },
                    daysOfTheWeek: {
                        required: true
                    },
                    date: 'required'
                },
                submitHandler: this.save
            });

            return this;

        },

        save: function(form) {

            var serialized = this.serializeForm(form),
                date = new Date(),
                startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]')),
                endTime = this._getTime(this.$('[data-role=timepicker][name=endTime]'));

            if(+serialized.type === TaxiHail.Rate.type.recurring ) {
                serialized.daysOfTheWeek =  _([serialized.daysOfTheWeek])
                    .flatten()
                    .reduce(function(memo, num){ return memo + (1<<num); }, 0);
            } else {
                date = new Date(this.$('[data-role=datepicker]').data('datepicker').date.toString());
                startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]'), date);
                endTime   = this._getTime(this.$('[data-role=timepicker][name=endTime]'  ), date);
            }

            if(startTime > endTime) {
                endTime.setDate(endTime.getDate() + 1);
            }

            serialized.startTime = TaxiHail.date.toISO8601(startTime);
            serialized.endTime   = TaxiHail.date.toISO8601(endTime);

            this.model.save(serialized, {
                success: _.bind(function(model) {
                    this.collection.add(model);
                    TaxiHail.app.navigate('rates', {trigger: true});
                }, this),
                error: function(model, xhr, options) {
                    this.$(':submit').button('reset');

                    var alert = new TaxiHail.AlertView({
                        message: TaxiHail.localize(xhr.statusText),
                        type: 'error'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.errors').html(alert.render().el);
                }
            });
            
        },

        _getTime: function($timepicker, date) {
            var timepicker = $timepicker.data('timepicker'),
                hour = timepicker.hour;

            date = _.isDate(date) ? new Date(date) : new Date();

            if(timepicker.meridian.toUpperCase() === "PM") {
                if(hour < 12) {
                    hour+= 12;
                }
            } else if (timepicker.meridian.toUpperCase() === "AM") {
                if(hour === 12) {
                    hour = 0;
                }
            }

            date.setHours(hour);
            date.setMinutes(timepicker.minute);

            return date;
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());