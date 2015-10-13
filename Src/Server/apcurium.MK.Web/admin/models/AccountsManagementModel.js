(function () {

	TaxiHail.AccountsManagementModel = Backbone.Model.extend({

		setAccounts:function(accountsList)
		{
			var countryCodes = this.get("countryCodes");

			var currentAccountID = this.get("currentAccountID").replace(/-/g, "").toLowerCase();

			for (i = 0; i < accountsList.length; i++)
			{
				var id = accountsList[i].id.replace(/-/g, "").toLowerCase();

				if (id != currentAccountID)
				{
					accountsList[i].currentAccount = false;
				}
				else
				{
					accountsList[i].currentAccount = true;
				}

				for (i1 = 0; i1 < countryCodes.length; i1++)
				{
					if (countryCodes[i1].CountryISOCode.Code == accountsList[i].settings.country.code)
					{
						accountsList[i].settings.dialCode = countryCodes[i1].CountryDialCode;
						break;
					}
				}
			}

			this.set("accounts", accountsList);
		},

		getAccount:function(id)
		{
			var accounts = this.get("accounts");

			if (accounts != undefined && accounts != null)
			{
				for (i = 0; i < accounts.length; i++)
				{
					if (accounts[i].id == id)
					{
						return accounts[i];
					}
				}
			}

			return null;
		},

		setSearchCriteria: function (criteria)
		{
			this.set("criteria", criteria);
		},

		getSearchCriteria: function ()
		{
			return this.get("criteria");
		},

		findButtonDisable: function (disable)
		{
			this.set("findButtonDisable", disable);
		},

		getAccountsWithSearchCriteria: function (criteria, viewObject, completeCallback)
		{
			criteria = criteria.toString();

			if (criteria.length > 0)
			{
				$.ajax({
					type: 'GET',
					url: "../api/account/findaccounts/" + criteria,
					data: { format: "json" },
					dataType: "application/json",
					complete: function (data) {
						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		goToAccountManagement:function(accountID)
		{
			TaxiHail.AccountManagementAccount = this.getAccount(accountID);
			window.location.href = "#accountManagement";
		}
	});
}());