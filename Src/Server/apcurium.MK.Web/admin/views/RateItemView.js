(function(){
    
    TaxiHail.RateItemView = TaxiHail.TemplatedView.extend({

        tagName: 'tr',

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
        }

    });

}());