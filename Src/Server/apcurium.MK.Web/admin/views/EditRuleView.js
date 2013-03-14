(function(){
    

    var View = TaxiHail.EditRuleView = TaxiHail.TemplatedView.extend({

        events: {
            'change [data-role=timepicker]': 'ontimepickerchange',
            'click [data-action=saveEnable]': 'onSaveEnableClick',
            'click [data-action=saveDisable]': 'onSaveDisableClick'
        },

        render: function() {

            var daysOfTheWeek = this.model.get('daysOfTheWeek'),
                now = new Date(),
                today = new Date(now.getFullYear(), now.getMonth(), now.getDate()),
                data = this.model.toJSON();
            
            data.currentBookingChecked = data.appliesToCurrentBooking == true ? 'checked' : '';
            data.futureBookingChecked = data.appliesToFutureBooking == true ? 'checked' :  '';
            
            data.recurring = +this.model.get('type') === TaxiHail.Rule.type.recurring;
            data.isDefault = +this.model.get('type') === TaxiHail.Rule.type['default'];
            data.editMode = !this.model.isNew();

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
                    kilometricRate: {
                        required: true,
                        min:0
                    },
                    marginOfError: {
                        required: true,
                        min:0
                    },
                    passengerRate: {
                        required: true,
                        min: 0
                    },
                    'daysOfTheWeek': {
                        required: true,
                        minlength: 1
                    },
                    date: 'required'
                },
                errorPlacement: function($label, $element) {
                    if($element.attr('name') === 'daysOfTheWeek')
                    {
                        $element = $element.closest('.control-group').children().last();
                    }
                    $label.insertAfter($element);

                },
                submitHandler: this.save
            });

            return this;

        },
        
        onSaveEnableClick: function (e) {
            e.preventDefault();
            this.save(this.$('form[name="editRuleForm"]'),true);
        },
        
        onSaveDisableClick: function (e) {
            e.preventDefault();
            this.save(this.$('form[name="editRuleForm"]'),false);
        },

        save: function(form, isActive) {

            var serialized = this.serializeForm(form),
                date = new Date(),
                startTime,
                endTime;
            serialized.isActive = isActive == true ? true : false;
            serialized.appliesToCurrentBooking = $("#appliesToCurrentBooking").attr('checked')? true : false;
            serialized.appliesToFutureBooking = $("#appliesToFutureBooking").attr('checked')? true : false;
           /* if(+serialized.type) {
                // Not a default rate

                if(+serialized.type === TaxiHail.Rule.type.recurring ) {

                    startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]'));
                    endTime = this._getTime(this.$('[data-role=timepicker][name=endTime]'));
                    serialized.daysOfTheWeek =  _([serialized.daysOfTheWeek])
                        .flatten()
                        .reduce(function(memo, num){ return memo + (1<<num); }, 0);

                } else if(+serialized.type === TaxiHail.Rule.type.day) {
                    
                    date = new Date(this.$('[data-role=datepicker]').data('datepicker').date.toString());
                    startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]'), date);
                    endTime   = this._getTime(this.$('[data-role=timepicker][name=endTime]'  ), date);
                
                }

                if(startTime > endTime) {
                    endTime.setDate(endTime.getDate() + 1);
                }

                serialized.startTime = TaxiHail.date.toISO8601(startTime);
                serialized.endTime   = TaxiHail.date.toISO8601(endTime);
            }*/

            this.model.save(serialized, {
                success: _.bind(function(model) {
                    this.collection.add(model);
                    TaxiHail.app.navigate('rules', {trigger: true});
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

        ontimepickerchange: function(e) {
            var startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]')),
                endTime = this._getTime(this.$('[data-role=timepicker][name=endTime]'));

            if(endTime <= startTime) {
                this.$('.next-day-warning').removeClass('hidden');
            } else {
                this.$('.next-day-warning').addClass('hidden');
            }
        },

        _getTime: function($timepicker, date) {
            var timepicker = $timepicker.data('timepicker');

            if(!timepicker) {
                return date;
            }

            var hour = timepicker.hour;

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