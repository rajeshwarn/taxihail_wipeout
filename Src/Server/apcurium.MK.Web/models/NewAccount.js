(function () {
    
    TaxiHail.NewAccount = Backbone.Model.extend({
        
        url: "api/account/register",

        defaults: {
    		"email":  null,
    		"phone":  null,
    		"name":  null,
    		"password":  null,
    		"confirmPassword":  null
  		},


        validation: {
            name: {
		      required: true
		    },
		    phone: {
		        required: true,
		        pattern: /^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$/,
		        msg: 'phone number is not valid'
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