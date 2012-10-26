(function(){
    
    TaxiHail.RateItemView = TaxiHail.TemplatedView.extend({

        tagName: 'tr',

        events: {
            'click [data-action=delete]': 'ondelete'
        },

        initialize: function() {
            this.model.on('destroy', this.remove, this);
        },

        render: function() {

            var days = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'],
                selectedDays = [];
            _.each(days, function(dayName, index){
                var day = 1 << index;
                if( day === (this.model.get('daysOfTheWeek') & day) ) {
                    selectedDays.push(dayName);
                }
            }, this);

            var data = this.model.toJSON();
            data.daysOfTheWeek = selectedDays.join(' - ');

            this.$el.html(this.renderTemplate(data));

            return this;
        },

        ondelete: function(e) {
            e.preventDefault();
            this.model.destroy();
        }

    });

}());