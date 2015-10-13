(function ()
{
	// model properties
	// accountID - edited account iD
	// account - edited account
	// orders - list of orders of edited account
	// currentAccountID - it's account id of the user which is connected with web site (logged user)

	TaxiHail.AccountManagementModel = Backbone.Model.extend({

		getAccountWithID: function (accountID, viewObject, completeCallback)
		{
			if (accountID && accountID.toString().length > 0)
			{
				var model = this;

				$.ajax({
					type: 'GET',
					url: "../api/account/findaccount/" + accountID.toString(),
					data: { format: "json" },
					dataType: "application/json",
					complete: function (data)
					{
						if (data.status == 200)
						{
							model.set("account", JSON.parse(data.responseText));

							var account = model.getAccount();
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
			return this.get("accountID");
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

		sendConfirmationCodeSMS: function (email, countryCode, phoneNumber, viewObject, completeCallback)
		{
			email = email.toString();

			if (email.length)
			{
				$.ajax({
					type: 'GET',
					url: "../api/account/getconfirmationcode/" + email + "/" + countryCode + "/" + phoneNumber,
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

		disableEmail: function (email, viewObject, completeCallback)
		{
			email = email.toString();

			if (email.length)
			{
				$.ajax({
					type: 'PUT',
					url: "../api/account/admindisable",
					data: { format: "json", accountEmail: email },
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