(function(){
    

    var View = TaxiHail.AddRecurringRateView  = TaxiHail.TemplatedView.extend({

        render: function() {

            var defaults = {
                flatRate: 0,
                pricePerPassenger: 0,
                distanceMultiplicator: 1,
                timeAdjustmentFactor: 1
            };

            this.$el.html(this.renderTemplate(defaults));

            this.$('[data-role=timepicker]').timepicker();

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
                    }
                },
                submitHandler: this.save
            });

            return this;

        },

        save: function(form) {

            var serialized = this.serializeForm(form);

            serialized.daysOfTheWeek =  _([serialized.daysOfTheWeek])
                .flatten()
                .reduce(function(memo, num){ return memo + (1<<num); }, 0);

            serialized.startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]'));
            serialized.endTime   = this._getTime(this.$('[data-role=timepicker][name=endTime]'));

            this.collection.create(serialized);
            TaxiHail.app.navigate('rates', {trigger: true});

        },

        _getTime: function($timepicker) {
            var timepicker = $timepicker.data('timepicker'),
                hour = timepicker.hour;

            if(timepicker.meridian.toUpperCase() === "PM") {
                if(hour < 12) {
                    hour+= 12;
                }
            } else if (timepicker.meridian.toUpperCase() === "AM") {
                if(hour === 12) {
                    hour = 0;
                }
            }

            return TaxiHail.date.toISO8601(new Date(1, 0, 1, hour, timepicker.minute, 0 ));
        }

    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}());