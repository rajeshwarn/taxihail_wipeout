(function(){

    var Rule = TaxiHail.Rule = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + '/admin/rules',
        defaults: function(){
            var today = new Date();
            return { name: '',
                message: '',                
                startTime: TaxiHail.date.toISO8601(new Date(today.getYear(), today.getMonth(), today.getDate())),
                endTime: TaxiHail.date.toISO8601(new Date(today.getYear(), today.getMonth(), today.getDate() + 1)),
                daysOfTheWeek: 0
            };
        }
    }, {
        type: {
            'default': 0,
            recurring: 1,
            date: 2
        }
          , 
        category: {
            disableRule: 0,
            warningRule: 1            
        }

    }
     );

    Rule.prototype.defaults.type = Rule.type.recurring;
    Rule.prototype.defaults.category = Rule.category.disableRule;

}());