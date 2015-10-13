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
			'click [data-action=unlinkIBSAccount]': 'unlinkIBSAccount'
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

			if (account && account.email && account.settings.country.code && account.settings.phone)
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
						viewObject.$('.errors').text(TaxiHail.localize('SendSMSError'))
					}
				});
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Email should not be empty'))
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
							viewObject.$('.errors').text(TaxiHail.localize('Error during enable/disable account'))
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
							viewObject.$('.errors').text(TaxiHail.localize('Error during enable/disable account'))
						}
					});
				}
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Email should not be empty'))
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
		}
	});
}());