(function () {

	var AccountModel = TaxiHail.AccountModel = Backbone.Model.extend({

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
						accountsList[i].dialCode = countryCodes[i1].CountryDialCode;
						break;
					}
				}
			}

			this.set("accounts", accountsList);
		},

		getAccount:function(email)
		{
			var accounts = this.get("accounts");

			if (accounts != undefined && accounts != null)
			{
				for (i = 0; i < accounts.length; i++)
				{
					if (accounts[i].email == email)
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

		sendConfirmationCodeSMS: function (email, viewObject, completeCallback)
		{
			email = email.toString();

			if (email.length)
			{
				$.ajax({
					type: 'GET',
					url: "../api/account/getconfirmationcode/" + email,
					data: { format: "json" },
					dataType: "application/json",
					complete: function (data) {
						if (completeCallback != undefined && completeCallback != null) {
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		enableEmail: function (email, viewObject, completeCallback)
		{
			email = email.toString();

			if (email.length)
			{
				$.ajax({
					type: 'PUT',
					url: "../api/account/adminenable",
					data: { format: "json", accountEmail: email },
					dataType: "application/json",
					complete: function (data) {
						if (completeCallback != undefined && completeCallback != null) {
							completeCallback(viewObject, data);
						}
					}
				});
			}

			/*var email = this.$('[name=email]').val();
			return $.ajax({
				type: 'PUT',
				url: '../api/account/adminenable',
				data: {
					accountEmail: email
				},
				dataType: 'json',
				success: _.bind(function () {
					this.$('.errors').text(TaxiHail.localize('confirmEmailSuccess'));
				}, this)
			}).fail(_.bind(function (e) {
				this.$('.errors').text(TaxiHail.localize('confirmEmailError'));
			}), this);*/




		},



		disableEmail: function (email, viewObject, completeCallback)
		{
			email = email.toString();

			if (email.length) {
				$.ajax({
					type: 'PUT',
					url: "../api/account/admindisable",
					data: { format: "json", accountEmail: email },
					dataType: "application/json",
					complete: function (data) {
						if (completeCallback != undefined && completeCallback != null) {
							completeCallback(viewObject, data);
						}
					}
				});


				/*e.preventDefault();
				var email = this.$('[name=email]').val();
				return $.ajax({
					type: 'PUT',
					url: '../api/account/admindisable',
					data: {
						accountEmail: email
					},
					dataType: 'json',
					success : _.bind(function() {
						this.$('.errors').text(TaxiHail.localize('disableEmailSuccess'));
					},this)
				}).fail(_.bind(function (e) {
					this.$('.errors').text(TaxiHail.localize('disableEmailError'));
				}),this);*/
			}
		},

		unlinkAccount: function (email, viewObject, completeCallback)
		{
			email = email.toString();

			if (email.length)
			{
				$.ajax({
					type: 'PUT',
					url: "../api/account/unlink",
					data: { format: "json", accountEmail: email },
					dataType: "application/json",
					complete: function (data) {
						if (completeCallback != undefined && completeCallback != null) {
							completeCallback(viewObject, data);
						}
					}
				});
			}
		}
	});
}());