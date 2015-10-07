﻿(function () {

	var View = TaxiHail.AccountManagementView = TaxiHail.TemplatedView.extend({

		model: null, //AccountModel.js

		tagName: 'form',
		className: 'form-horizontal',

		events:
		{
			'click [data-action=findUserWithSearchCriteria]': 'findUserWithSearchCriteria',
			'keyup [data-action=findUserWithSearchCriteriaEnterOnTextfield]': 'findUserWithSearchCriteria',
			'click [data-action=sendConfirmationCodeSMS]': 'sendConfirmationCodeSMS',
			'click [data-action=enableDisableAccount]': 'enableDisableAccount',
			'click [data-action=unlinkIBSAccount]': 'unlinkIBSAccount'
		},

		initialize: function (parameters)
		{
			model = parameters;
		},

		render: function ()
		{
			var data = model.toJSON();

			this.$el.html(this.renderTemplate(data));

			this.$("#searchcriteria").val(model.getSearchCriteria());

			return this;
		},

		updateAccountsList: function (accountsList)
		{
			model.setAccounts(accountsList);
			var searchCriteria = $("#searchcriteria").val();
			model.setSearchCriteria(searchCriteria);
			this.render();
		},

		findDisable:function(disable)
		{
			model.findButtonDisable(disable);
			$("#buttonFindUsers").prop("disabled", disable);
			$("#searchcriteria").prop("disabled", disable);
		},

		findUserWithSearchCriteriaInternal: function ()
		{
			var searchCriteria = $("#searchcriteria").val();
			model.setSearchCriteria(searchCriteria);

			this.updateAccountsList([]);

			if (searchCriteria.toString().length > 0)
			{
				this.findDisable(true);

				model.getAccountsWithSearchCriteria(searchCriteria, this, function (viewObject, data) {

					viewObject.findDisable.call(viewObject, false);

					if (data.status == 200)
					{
						viewObject.updateAccountsList.call(viewObject, JSON.parse(data.responseText));
					}
					else
					{
						viewObject.$('.errors').text(TaxiHail.localize('Error during users search'))
					}
				});
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Email should not be empty'))
			}
		},

		findUserWithSearchCriteria: function (e)
		{
			if (e.type == "click")
			{
				e.preventDefault();
				this.findUserWithSearchCriteriaInternal(this);
			}
		},

		sendConfirmationCodeSMS: function (e)
		{
			e.preventDefault();

			var email = e.currentTarget.attributes.email.nodeValue;
			var accid = e.currentTarget.attributes.accid.nodeValue;

			if (email != undefined && email != null && email.toString().length > 0)
			{
				var sendButton = document.getElementById("buttonSendSMS" + accid);
				sendButton.disabled = true;

				model.sendConfirmationCodeSMS(email, this, function (viewObject, data) {

					if (data.status == 200)
					{
						sendButton.innerText = TaxiHail.localize("Sent");
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

			var email = e.currentTarget.attributes.email.nodeValue;
			var accid = e.currentTarget.attributes.accid.nodeValue;

			if (email != undefined && email != null && email.toString().length > 0)
			{
				var enableDisableAccountButton = document.getElementById("buttonEnableDisableAccount" + accid);
				enableDisableAccountButton.disabled = true;

				var account = model.getAccount(accid);

				if (!account.disabledByAdmin)
				{
					model.disableEmail(email, this, function (viewObject, data) {

						if (data.status == 200)
						{
							viewObject.findUserWithSearchCriteriaInternal.call(viewObject);
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
					model.enableEmail(email, this, function (viewObject, data) {

						if (data.status == 200)
						{
							viewObject.findUserWithSearchCriteriaInternal.call(viewObject);
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

		unlinkIBSAccount:function(e)
		{
			e.preventDefault();

			var email = e.currentTarget.attributes.email.nodeValue;
			var accid = e.currentTarget.attributes.accid.nodeValue;

			if (email != undefined && email != null && email.toString().length > 0)
			{
				var unlinkIBSAccounButton = document.getElementById("buttonUnlinkIBSAccount" + accid);
				unlinkIBSAccounButton.disabled = true;

				model.unlinkAccount(email, this, function (viewObject, data) {

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