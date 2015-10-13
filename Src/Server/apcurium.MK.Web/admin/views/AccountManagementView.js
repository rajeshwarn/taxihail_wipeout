(function ()
{
	TaxiHail.AccountManagementView = TaxiHail.TemplatedView.extend({

		model: null, //AccountManagementModel.js

		tagName: 'form',
		className: 'form-horizontal',

		events:
		{
			'click [data-action=saveAccount]': 'saveAccount',
			'click [data-action=sendConfirmationCodeSMS]': 'sendConfirmationCodeSMS',
			'click [data-action=enableDisableAccount]': 'enableDisableAccount',
			'click [data-action=unlinkIBSAccount]': 'unlinkIBSAccount',
			'click [data-action=deleteCreditCardsInfo]': 'deleteCreditCardsInfo'
		},

		initialize: function (accountManagementModel)
		{
			model = accountManagementModel;

			this.refresh(this);
		},

		refresh:function(viewObject)
		{
			model.getAccountWithID(model.getAccountID(), this, function (viewObject, data)
			{
				model.getUserOrders(viewObject, function (viewObject1, data)
				{
					viewObject1.render();
				});
			});
		},

		render: function ()
		{
			var account = model.getAccount();

			if (account)
			{
				this.$el.html(this.renderTemplate(model.toJSON()));
				this.$("#countryСode").val(account.settings.country.code).selected = "true";
			}

			return this;
		},

		saveAccount:function(e)
		{
			e.preventDefault();
		},

		sendConfirmationCodeSMS: function (e)
		{
			e.preventDefault();

			var account = model.getAccount(e.currentTarget.attributes.accountid.nodeValue);

			if (account && account.email && account.settings.country.code && account.settings.phone
				&& account.email.toString().length > 0 && account.settings.country.code.toString().length > 0 && account.settings.phone.toString().length > 0)
			{
				var sendRef = document.getElementById("refSendSMS" + account.id);
				sendRef.disabled = true;
				sendRef.classList.add("label-account-linkitem-disabled");

				model.sendConfirmationCodeSMS(account.email, account.settings.country.code, account.settings.phone, this, function (viewObject, data)
				{
					if (data.status == 200)
					{
						sendRef.innerText = TaxiHail.localize("Code will be send shortly");
					}
					else
					{
						sendRef.classList.remove("label-account-linkitem-disabled");
						sendRef.disabled = false;
						viewObject.$('.errors').text(TaxiHail.localize('SendSMSError'));
					}
				});
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Email, country code and phone number should not be empty'));
			}
		},

		enableDisableAccount: function (e)
		{
			e.preventDefault();

			var account = model.getAccount(e.currentTarget.attributes.accountid.nodeValue);

			if (account)
			{
				var enableDisableAccountRef = document.getElementById("refEnableDisableAccount" + account.id);
				enableDisableAccountRef.disabled = true;
				enableDisableAccountRef.classList.add("label-account-linkitem-disabled");

				if (!account.disabledByAdmin)
				{
					model.disableEmail(account.email, this, function (viewObject, data)
					{
						if (data.status == 200)
						{
							window.setTimeout(function () { viewObject.refresh(viewObject); }, 1000);
						}
						else
						{
							enableDisableAccountRef.classList.remove("label-account-linkitem-disabled");
							enableDisableAccountRef.disabled = false;
							viewObject.$('.errors').text(TaxiHail.localize('Error during enable/disable account'));
						}
					});
				}
				else
				{
					model.enableEmail(account.email, this, function (viewObject, data)
					{
						if (data.status == 200)
						{
							window.setTimeout(function () { viewObject.refresh(viewObject); }, 1000);
						}
						else
						{
							enableDisableAccountRef.classList.remove("label-account-linkitem-disabled");
							enableDisableAccountRef.disabled = false;
							viewObject.$('.errors').text(TaxiHail.localize('Error during enable/disable account'));
						}
					});
				}
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Account should not be empty'));
			}
		},

		unlinkIBSAccount: function (e)
		{
			e.preventDefault();

			var account = model.getAccount(e.currentTarget.attributes.accountid.nodeValue);

			if (account)
			{
				var unlinkIBSAccounRef = document.getElementById("refUnlinkIBSAccount" + account.id);
				unlinkIBSAccounRef.disabled = true;
				unlinkIBSAccounRef.classList.add("label-account-linkitem-disabled");

				model.unlinkAccount(account.email, this, function (viewObject, data)
				{
					if (data.status == 200)
					{
						unlinkIBSAccounRef.innerText = TaxiHail.localize("IBS Account Unlinked");
					}
					else
					{
						unlinkIBSAccounRef.classList.remove("label-account-linkitem-disabled");
						unlinkIBSAccounRef.disabled = false;
						viewObject.$('.errors').text(TaxiHail.localize('unlinkAccountError'))
					}
				});
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Email should not be empty'))
			}
		},

		deleteCreditCardsInfo: function (e)
		{
			e.preventDefault();

			var account = model.getAccount(e.currentTarget.attributes.accountid.nodeValue);

			TaxiHail.confirm({
				title: "Credit card removal",
				message: "Confirm removing all credit cards for user " + account.name
			}).on('ok', function ()
			{
				var deleteCreditCardsInfoRef = document.getElementById("refDeleteCreditCardsInfo" + account.id);
				deleteCreditCardsInfoRef.disabled = true;
				deleteCreditCardsInfoRef.classList.add("label-account-linkitem-disabled");

				model.deleteAccountCreditCards(account.id, this, function (viewObject, data)
				{
					if (data.status == 200)
					{
						deleteCreditCardsInfoRef.innerText = "Credit cards info deleted";
					}
					else
					{
						deleteCreditCardsInfoRef.disabled = false;
						deleteCreditCardsInfoRef.classList.remove("label-account-linkitem-disabled");
						viewObject.$('.errors').text('Error during credit cards info removing');
					}
				});
			}, this);
		}
	});
}());