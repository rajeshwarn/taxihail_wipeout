(function () {
    
    TaxiHail.NewAccount = Backbone.Model.extend({
        
        url: "api/account/register",

        defaults: {
    		"email":  null,
    		"phone":  null,
    		"fullname":  null,
    		"password":  null,
    		"confirmPassword":  null
  		},


        validation: {
		    fullname: {
		      required: true
		    },
		    phone: {
		      required: true
		    },
		    email: {
		      pattern: 'email'
		    },
		    password: {
		      required: true
		    },
		    confirmPassword:{
		    	equalTo: 'password'
		    }
		}
    });

})();