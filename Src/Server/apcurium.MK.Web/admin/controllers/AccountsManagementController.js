(function () {

	var accountsManagementModel = new TaxiHail.AccountsManagementModel({
		countryCodes: TaxiHail.extendSpacesForCountryDialCode(TaxiHail.countryCodes),
		defaultCountryCode: TaxiHail.parameters.defaultCountryCode,
		currentAccountID: TaxiHail.parameters.currentAccountID // it's account id of the user which is connected with web site
	});

	TaxiHail.AccountsManagementController = TaxiHail.Controller.extend({

		initialize: function ()
		{
			this.ready();
		},

		accountsManagement: function ()
		{
			return new TaxiHail.AccountsManagementView(accountsManagementModel);
		},

		accountManagement: function ()
		{
			var accountID = null;

			if (TaxiHail.AccountManagementAccount != undefined && TaxiHail.AccountManagementAccount != null)
			{
				accountID = TaxiHail.AccountManagementAccount.id;
			}

			var accountManagementModel = new TaxiHail.AccountManagementModel({
					countryCodes: TaxiHail.extendSpacesForCountryDialCode(TaxiHail.countryCodes),
					currentAccountID: TaxiHail.parameters.currentAccountID, // it's account id of the user which is connected with web site
					accountID: accountID
				});

			return new TaxiHail.AccountManagementView(accountManagementModel);
		}
});
}());