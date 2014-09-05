(function () {

    TaxiHail.EmailTemplates = Backbone.Model.extend({
        urlRoot: TaxiHail.parameters.apiRoot + "/admin/testemail/templates"
    });

}());