(function () {

	var accountModel = new TaxiHail.AccountModel({
		countryCodes: TaxiHail.extendSpacesForCountryDialCode(TaxiHail.countryCodes),
		defaultCountryCode: TaxiHail.parameters.defaultCountryCode
	});

	var Controller = TaxiHail.AccountController = TaxiHail.Controller.extend({

		initialize: function ()
		{
			this.ready();
		},

		accountManagement: function ()
		{
			return new TaxiHail.AccountManagementView(accountModel);
		}
	});
}());