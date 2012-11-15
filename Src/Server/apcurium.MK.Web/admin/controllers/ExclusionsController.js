(function(){

    var Controller = TaxiHail.ExclusionsController = TaxiHail.Controller.extend({
        initialize: function() {

            this.referenceData = new TaxiHail.ReferenceData();
            this.exclusions = new Backbone.Model();
            this.exclusions.urlRoot = TaxiHail.parameters.apiRoot + '/admin/exclusions';
            
            $.when(this.referenceData.fetch({
                data: {
                    withoutFiltering: true
                }}), this.exclusions.fetch()).then(this.ready);
        },

        index: function() {
            return new TaxiHail.ManageExclusionsView({
                model: this.referenceData,
                exclusions: this.exclusions
            });
        }
    });


}());