(function ()
{
	// model properties
	// accountID - edited account iD
	// account - edited account
	// orders - list of orders of edited account
	// countryCodes - list of country codes
	// currentAccountID - it's account id of the user which is connected with web site (logged user)

	TaxiHail.AccountManagementModel = Backbone.Model.extend({

		getAccountWithID: function (viewObject, completeCallback)
		{
			var accountId = this.getAccountID();

			if (accountId && accountId.toString().length > 0)
			{
				var model = this;

				$.ajax({
					type: 'GET',
					url: "../api/account/findaccount/" + accountId.toString(),
					data: { format: "json" },
					dataType: "application/json",
					complete: function (data)
					{
						if (data.status == 200)
						{
							model.set("account", JSON.parse(data.responseText));

							var account = model.getAccount();

							var currentAccountID = model.get("currentAccountID").replace(/-/g, "").toLowerCase();

							var id = account.id.replace(/-/g, "").toLowerCase();

							if (id != currentAccountID)
							{
								account.currentAccount = false;
							}
							else
							{
								account.currentAccount = true;
							}

							var creationDate = new Date(account.creationDate);
							account.creationDateText = creationDate.toLocaleDateString("en-US") + " " + creationDate.toLocaleTimeString("en-US");
						}

						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		getUserOrders: function (viewObject, completeCallback)
		{
			var account = this.getAccount()

			if (account)
			{
				var model = this;

				$.ajax({
					type: 'GET',
					url: "../api/account/orderswithuserid/" + account.id,
					data: { format: "json" },
					dataType: "application/json",
					complete: function (data)
					{
						if (data.status == 200)
						{
							model.setOrders(JSON.parse(data.responseText));
						}

						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		getAccount: function ()
		{
			return this.get("account");
		},

		getAccountID: function ()
		{
			var accountId = this.get("accountID");

			if (accountId == undefined || accountId == null || accountId.toString.length == 0)
			{
				var parametersStartIndex = window.location.href.indexOf("?");

				if (parametersStartIndex >= 0)
				{
					var parametersText = window.location.href.substring(parametersStartIndex + 1, window.location.href.length);
					var parameters = parametersText.split("=");

					if (parameters.length % 2 == 0)
					{
						for (i = 0; i < parameters.length; i = i + 2)
						{
							if (parameters[i] == "accountId")
							{
								this.set("accountID", parameters[i + 1]);
								break;
							}
						}
					}
				}
			}

			return this.get("accountID");
		},

		getCountryCodes:function()
		{
			return this.get("countryCodes");
		},

		setOrders: function (orders)
		{
			for (i = 0; i < orders.length; i++)
			{
				var createdDate = new Date(orders[i].createdDate);
				var pickupDate = new Date(orders[i].pickupDate);

				orders[i].createdDateText = createdDate.toLocaleDateString("en-US");
				orders[i].createdTimeText = createdDate.toLocaleTimeString("en-US");

				orders[i].pickupDateText = pickupDate.toLocaleDateString("en-US");
				orders[i].pickupTimeText = pickupDate.toLocaleTimeString("en-US");

				switch (orders[i].status)
				{
					case "Unknown":
						orders[i].statusText = "Unknown";
						break;

					case "Pending":
						orders[i].statusText = "Pending";
						break;

					case "Created":
						orders[i].statusText = "Created";
						break;

					case "Canceled":
						orders[i].statusText = "Cancelled";
						break;

					case "Completed":
						orders[i].statusText = "Completed";
						break;

					case "Removed":
						orders[i].statusText = "Removed";
						break;

					case "TimedOut":
						orders[i].statusText = "Timed out";
						break;

					case "WaitingForPayment":
						orders[i].statusText = "Waiting for payment";
						break;
				}
			}

			model.set("orders", orders);
		},

		saveAccount: function (accountUpdateRequest, viewObject, completeCallback) {
			$.ajax({
				type: 'PUT',
				url: "../api/account/update/" + accountUpdateRequest.accountId,
				contentType: 'application/json; charset=UTF-8',
				dataType: "json",
				data: JSON.stringify(accountUpdateRequest),
				complete: function (data)
				{
					if (completeCallback != undefined && completeCallback != null)
					{
						completeCallback(viewObject, data);
					}
				}
			});
		},

		sendConfirmationCodeSMS: function (viewObject, completeCallback)
		{
			var account = this.getAccount();

			if (account && account.email && account.email.length > 0 && account.settings.country && account.settings.country.code && account.settings.country.code.length > 0
				&& account.settings.phone && account.settings.phone.length > 0)
			{
				$.ajax({
					type: 'GET',
					url: "../api/account/getconfirmationcode/" + account.email + "/" + account.settings.country.code + "/" + account.settings.phone,
					data: { format: "json" },
					dataType: "application/json",
					complete: function (data)
					{
						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		resetPassword: function (viewObject, completeCallback)
		{
			var account = this.getAccount();

			if (account && account.email && account.email.length > 0)
			{
				$.ajax({
					type: 'POST',
					url: "../api/account/resetpassword/" + account.email,
					contentType: 'application/json; charset=UTF-8',
					dataType: "json",
					complete: function (data)
					{
						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		enableEmail: function (viewObject, completeCallback)
		{
			var account = this.getAccount();

			if (account && account.email && account.email.length > 0)
			{
				$.ajax({
					type: 'PUT',
					url: "../api/account/adminenable",
					data: { format: "json", accountEmail: account.email },
					dataType: "json",
					complete: function (data)
					{
						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		disableEmail: function (viewObject, completeCallback)
		{
			var account = this.getAccount();

			if (account && account.email && account.email.length > 0)
			{
				$.ajax({
					type: 'PUT',
					url: "../api/account/admindisable",
					data: { format: "json", accountEmail: account.email },
					dataType: "json",
					complete: function (data)
					{
						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		unlinkAccount: function (viewObject, completeCallback)
		{
			var account = this.getAccount();

			if (account && account.email && account.email.length > 0)
			{
				$.ajax({
					type: 'PUT',
					url: "../api/account/unlink",
					data: { format: "json", accountEmail: account.email },
					dataType: "json",
					complete: function (data)
					{
						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		},

		deleteAccountCreditCards: function (viewObject, completeCallback)
		{
			var account = this.getAccount();

			if (account && account.id)
			{
				$.ajax({
					type: 'DELETE',
					url: "../api/admin/deleteAllCreditCards/" + account.id.toString(),
					data: { format: "json" },
					dataType: "json",
					complete: function (data)
					{
						if (completeCallback != undefined && completeCallback != null)
						{
							completeCallback(viewObject, data);
						}
					}
				});
			}
		}
	});
}());