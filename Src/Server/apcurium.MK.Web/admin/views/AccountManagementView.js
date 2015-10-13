(function ()
{
	TaxiHail.AccountManagementView = TaxiHail.TemplatedView.extend({

		model: null, //AccountManagementModel.js

		tagName: 'form',
		className: 'form-horizontal',

		events:
		{
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

		sendConfirmationCodeSMS: function (e)
		{
			e.preventDefault();

			var account = model.getAccount(e.currentTarget.attributes.accountid.nodeValue);

			if (account && account.email && account.settings.country.code && account.settings.phone
				&& account.email.toString().length > 0 && account.settings.country.code.toString().length > 0 && account.settings.phone.toString().length > 0)
			{
				var sendButton = document.getElementById("buttonSendSMS" + account.id);
				sendButton.disabled = true;

				model.sendConfirmationCodeSMS(account.email, account.settings.country.code, account.settings.phone, this, function (viewObject, data)
				{
					if (data.status == 200)
					{
						sendButton.innerText = TaxiHail.localize("Code will be send shortly");
					}
					else
					{
						sendButton.disabled = false;
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
				var enableDisableAccountButton = document.getElementById("buttonEnableDisableAccount" + account.id);
				enableDisableAccountButton.disabled = true;

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
							enableDisableAccountButton.disabled = false;
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
							enableDisableAccountButton.disabled = false;
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
				var unlinkIBSAccounButton = document.getElementById("buttonUnlinkIBSAccount" + account.id);
				unlinkIBSAccounButton.disabled = true;

				model.unlinkAccount(account.email, this, function (viewObject, data)
				{
					if (data.status == 200)
					{
						unlinkIBSAccounButton.innerText = TaxiHail.localize("IBS Account Unlinked");
					}
					else
					{
						unlinkIBSAccounButton.disabled = false;
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
				var buttonDeleteCreditCardsInfo = document.getElementById("buttonDeleteCreditCardsInfo" + account.id);
				buttonDeleteCreditCardsInfo.disabled = true;
				model.deleteAccountCreditCards(account.id, this, function (viewObject, data)
				{
					if (data.status == 200)
					{
						buttonDeleteCreditCardsInfo.innerText = "Credit cards info deleted";
					}
					else
					{
						buttonDeleteCreditCardsInfo.disabled = false;
						viewObject.$('.errors').text('Error during credit cards info removing');
					}
				});
			}, this);
		}
	});
}());