(function(){

    var Controller = TaxiHail.ExclusionsController = TaxiHail.Controller.extend({
        initialize: function() {

            this.referenceData = new TaxiHail.ReferenceData();
            $.when(this.referenceData.fetch()).then(this.ready);

        },

        index: function() {
            return this.view = new TaxiHail.ManageExclusionsView({
                model: this.referenceData
            });
        }
    });


}());