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
                this.model.set('pickupDate', toISO8601(pickupDate));
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

    function toISO8601(date) {
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
    }

}());