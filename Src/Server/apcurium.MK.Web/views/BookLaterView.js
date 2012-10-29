(function(){

    TaxiHail.BookLaterView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        className: 'form-horizontal',

        events: {
            'submit': 'onsubmit'
        },

        render: function() {

            var now = new Date(),
                today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

            this.$el.html(this.renderTemplate(this.model.toJSON()));
            this.$('[data-role=datepicker]').datepicker().datepicker('setValue', today);
            this.$('[data-role=timepicker]').timepicker({
                defaultTime: 'current+30'
            });

            return this;
        },

        onsubmit: function(e) {
            e.preventDefault();
            var date = this.$('[data-role=datepicker]').data('datepicker').date,
                hour = this.$('[data-role=timepicker]').data('timepicker').hour,
                minute = this.$('[data-role=timepicker]').data('timepicker').minute,
                meridian = this.$('[data-role=timepicker]').data('timepicker').meridian;

            if(meridian.toUpperCase() === "PM") {
                if(hour < 12) {
                    hour+= 12;
                }
            } else if(meridian.toUpperCase() === "AM") {
                if(hour === 12) {
                    hour = 0;
                }
            }

            var pickupDate = new Date(date.toString());
            pickupDate.setHours(hour);
            pickupDate.setMinutes(minute);

            if(this.validate(pickupDate)){
                this.model.set('pickupDate', TaxiHail.date.toISO8601(pickupDate));
                this.model.saveLocal();
                TaxiHail.app.navigate('confirmationbook', { trigger: true });
            }

        },

        validate: function(date) {
            var now = new Date();
            now.setMinutes(now.getMinutes() + 2);

            var isInFuture = now < date;

            if(!isInFuture) {
                this.showError(this.localize("error.PickupDateMustBeInFuture"));
            }

            return isInFuture;
        },

        showError: function (error) {
            var $alert = $('<div class="alert alert-error" />').text(error);
            this.$('.errors').html($alert);
        }

    });

}());