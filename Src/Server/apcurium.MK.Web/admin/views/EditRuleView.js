(function(){
    

    var View = TaxiHail.EditRuleView = TaxiHail.TemplatedView.extend({
        events: {
            'change [data-role=timepicker]': 'ontimepickerchange',
            'click [data-action=saveEnable]': 'onSaveEnableClick',
            'click [data-action=saveDisable]': 'onSaveDisableClick',
            'click [data-action=eraseStartTime]': 'onEraseStartTimeClick',
            'click [data-action=eraseEndTime]': 'onEraseEndTimeClick'
        },

        render: function() {
            var daysOfTheWeek = this.model.get('daysOfTheWeek'),
                now = new Date(),
                today = new Date(now.getFullYear(), now.getMonth(), now.getDate()),
                data = this.model.toJSON();
            
            data.highestPriority = _.max(_.pluck(this.collection.toJSON(), 'priority')) +1;
            data.currentBookingChecked = data.appliesToCurrentBooking == true ? 'checked' : '';
            data.futureBookingChecked = data.appliesToFutureBooking == true ? 'checked' : '';

            data.appliesToPickupChecked = data.appliesToPickup == true ? 'checked' : '';
            data.appliesToDropoffChecked = data.appliesToDropoff == true ? 'checked' : '';

            data.zoneRequiredChecked = data.zoneRequired == true ? 'checked' : '';
            
            data.recurring = +this.model.get('type') === TaxiHail.Rule.type.recurring;
            data.isDefault = +this.model.get('type') === TaxiHail.Rule.type['default'];
            data.isDay = +this.model.get('type') === TaxiHail.Rule.type.date;
            data.isWarning = +this.model.get('category') === TaxiHail.Rule.category.warningRule;
            data.isDisable = +this.model.get('category') === TaxiHail.Rule.category.disableRule;
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
            
            if (data.startTime != null) {
                var niceStartTime = TaxiHail.date.ISO8601toJs(data.startTime);
                this.$('[data-role=datepicker][name=startDate]').datepicker().datepicker('setValue', niceStartTime);
            }
            if (data.endTime != null) {
                var niceEndTime = TaxiHail.date.ISO8601toJs(data.endTime);
                this.$('[data-role=datepicker][name=endDate]').datepicker().datepicker('setValue', niceEndTime);
            }

            this.validate({
                rules: {
                    name: 'required',
                    message: 'required',
                    startTime: 'required',
                    endTime: 'required',
                  
                    priority: {
                        required: true,
                        number: true,
                        min:0
                    },
                    marginOfError: {
                        required: true,
                        min:0
                    },
                    kilometerIncluded: {
                        required: true,
                        min: 0
                    },
                    perMinuteRate: {
                        required: true,
                        min: 0
                    },
                    'daysOfTheWeek': {
                        required: true,
                        minlength: 1
                    }
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
            $('input[name="isActive"]').val(true);
            this.model.set('isActive',true);
            $('form[name="editRuleForm"]').submit();
        },
        
        onSaveDisableClick: function (e) {
            e.preventDefault();
            $('input[name="isActive"]').val(false);
            this.model.set('isActive',false);
            $('form[name="editRuleForm"]').submit();
        },

        save: function(form) {
                var serialized = this.serializeForm(form),
                startDate = new Date(),
                endDate = new Date(),
                startTime,
                endTime;
                serialized.category = +this.model.get('category');
                serialized.appliesToCurrentBooking = $("#appliesToCurrentBooking").attr('checked') ? true : false;
                serialized.appliesToFutureBooking = $("#appliesToFutureBooking").attr('checked') ? true : false;

                serialized.appliesToPickup = $("#appliesToPickup").attr('checked') ? true : false;
                serialized.appliesToDropoff = $("#appliesToDropoff").attr('checked') ? true : false;

                serialized.zoneRequired = $("#zoneRequired").attr('checked') ? true : false;

                if (+serialized.type) {
                    // Not a default rate

                    if (+serialized.type === TaxiHail.Rule.type.recurring) {
                        startDate = new Date(this.$('[data-role=datepicker][name=startDate]').data('datepicker').date.toString());
                        endDate = new Date(this.$('[data-role=datepicker][name=endDate]').data('datepicker').date.toString());

                        var startDateTxt = this.$('#book-later-date-start').val();
                        var endDateTxt = this.$('#book-later-date-end').val();
                        if (startDateTxt != "") {
                            startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]'), startDate);
                            serialized.activeFrom = TaxiHail.date.toISO8601(startTime);
                        } else {
                            startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]'));
                            
                        }
                        if (endDateTxt != "") {
                            endTime = this._getTime(this.$('[data-role=timepicker][name=endTime]'), endDate);
                            serialized.activeTo = TaxiHail.date.toISO8601(endTime);
                        } else {
                            endTime = this._getTime(this.$('[data-role=timepicker][name=endTime]'));
                        }
                        serialized.daysOfTheWeek = _([serialized.daysOfTheWeek])
                            .flatten()
                            .reduce(function (memo, num) { return memo + (1 << num); }, 0);

                    } else if (+serialized.type === TaxiHail.Rule.type.date) {

                        startDate = new Date(this.$('[data-role=datepicker][name=startDate]').data('datepicker').date.toString());
                        endDate = new Date(this.$('[data-role=datepicker][name=endDate]').data('datepicker').date.toString());
                        startTime = this._getTime(this.$('[data-role=timepicker][name=startTime]'), startDate);
                        endTime = this._getTime(this.$('[data-role=timepicker][name=endTime]'), endDate);
                        serialized.activeFrom = TaxiHail.date.toISO8601(startTime);
                        serialized.activeTo = TaxiHail.date.toISO8601(endTime);
                    }

                    serialized.startTime = TaxiHail.date.toISO8601(startTime);
                    serialized.endTime = TaxiHail.date.toISO8601(endTime);
                    
                }

                this.model.save(serialized, {
                    success: _.bind(function (model) {
                        this.collection.add(model);
                        TaxiHail.app.navigate('rules', { trigger: true });
                    }, this),
                    error: function (model, xhr, options) {
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
        
        onEraseStartTimeClick:function(e) {
            e.preventDefault();
            this.$('#book-later-date-start').val('');
        },
        onEraseEndTimeClick: function (e) {
            e.preventDefault();
            this.$('#book-later-date-end').val('');
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