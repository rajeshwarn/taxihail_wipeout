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
            var date = this.$('[data-role=datepicker] :text').val();
            var time = this.$('[data-role=timepicker]').val();

            this.model.set('pickupDate', date + 'T' + time);
            this.model.saveLocal();
            TaxiHail.app.navigate('confirmationbook', { trigger: true });

        }

    });

}());