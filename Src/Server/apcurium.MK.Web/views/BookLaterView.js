(function(){

    TaxiHail.BookLaterView = TaxiHail.TemplatedView.extend({

        tagName: 'form',

        events: {
            'submit': 'onsubmit'
        },

        render: function() {

            var now = new Date();
            var data = _.extend(this.model.toJSON(), {
                today: now.getFullYear() + '-' + (now.getMonth() + 1) + '-' + now.getDate()
            });

            this.$el.html(this.renderTemplate(data));
            this.$('[data-role=datepicker]').datepicker();
            this.$('[data-role=timepicker]').timepicker();

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


            this.model.set('pickupDate', toISO8601(pickupDate));
            this.model.saveLocal();
            TaxiHail.app.navigate('confirmationbook', { trigger: true });

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