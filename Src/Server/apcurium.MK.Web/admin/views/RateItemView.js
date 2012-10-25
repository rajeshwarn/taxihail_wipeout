(function(){
    
    TaxiHail.RateItemView = TaxiHail.TemplatedView.extend({

        tagName: 'li',

        render: function() {

            var days = ['Sunday', 'Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday'],
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