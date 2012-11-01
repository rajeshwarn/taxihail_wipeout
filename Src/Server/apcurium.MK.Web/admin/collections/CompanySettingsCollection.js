(function () {

    TaxiHail.CompanySettingsCollection = Backbone.Collection.extend({
        model: TaxiHail.CompanySetting,
        url: TaxiHail.parameters.apiRoot + "/settings",

        batchSave: function(settings) {

            var collection = this;

            return $.ajax({
                type: 'POST',
                url:this.url,
                data: JSON.stringify({appSettings: _.reduce(settings, function(memo, item){
                    memo[item.key] = item.value;
                    return memo;
                }, {})}),
                contentType: 'application/json'
            })
                .done(function(){

                    _.each(settings, function(setting){
                        var model = collection.get(setting.key);
                        if(model) {
                            model.set(setting);
                        } else {
                            collection.add(setting);
                        }
                    });

                });
        }
    });

}());