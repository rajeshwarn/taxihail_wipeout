(function () {
    
    TaxiHail.NewAccount = Backbone.Model.extend({
        
        url: "api/account/register",
       
        validate: function (attrs) {
            var errors = [];
            if (!attrs.email) errors.push({ errorCode: 'error.EmailRequired' });
            
            if (errors.length) return { errors: errors };
        }

    });

})();