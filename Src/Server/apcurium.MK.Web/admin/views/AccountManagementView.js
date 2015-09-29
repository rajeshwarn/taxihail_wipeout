(function () {

	var View = TaxiHail.AccountManagementView = TaxiHail.TemplatedView.extend({

		model: null, //AccountModel.js

		tagName: 'form',
		className: 'form-horizontal',

		events:
		{
			'click [data-action=findUserWithPhoneNumber]': 'findUserWithPhoneNumber',
			'keyup [data-action=findUserWithPhoneNumberEnterOnTextfield]': 'findUserWithPhoneNumber',
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

			this.$("#phonenumber").val(model.getPhoneNumber());

			return this;
		},

		updateAccountsList: function (accountsList)
		{
			model.setAccounts(accountsList);
			var phoneNumber = $("#phonenumber").val();
			model.setPhoneNumber(phoneNumber);
			this.render();
		},

		findDisable:function(disable)
		{
			model.findButtonDisable(disable);
			$("#buttonFindUsers").prop("disabled", disable);
			$("#phonenumber").prop("disabled", disable);
		},

		findUserWithPhoneNumberInternal: function ()
		{
			var phoneNumber = $("#phonenumber").val();
			model.setPhoneNumber(phoneNumber);

			this.updateAccountsList([]);

			if (phoneNumber.toString().length > 0)
			{
				this.findDisable(true);

				model.getAccountsWithPhoneNumber(phoneNumber, this, function (viewObject, data) {

					viewObject.findDisable.call(false);

					if (data.status == 200) {
						viewObject.updateAccountsList.call(viewObject, JSON.parse(data.responseText));
					}
				});
			}
		},

		findUserWithPhoneNumber: function (e)
		{
			if (e.type == "click")
			{
				e.preventDefault();
				this.findUserWithPhoneNumberInternal(this);
			}
		},

		sendConfirmationCodeSMS: function (e)
		{
			e.preventDefault();

			var email = e.currentTarget.attributes.email.nodeValue;

			var sendButton = document.getElementById("buttonSendSMS" + email);
			sendButton.disabled = true;

			model.sendConfirmationCodeSMS(email, this, function (viewObject, data) {
				sendButton.innerText = TaxiHail.localize("Sent");
			});
		},

		enableDisableAccount: function (e)
		{
			e.preventDefault();

			var email = e.currentTarget.attributes.email.nodeValue;

			var enableDisableAccountButton = document.getElementById("buttonEnableDisableAccount" + email);
			enableDisableAccountButton.disabled = true;

			var account = model.getAccount(email);

			if (!account.disabledByAdmin)
			{
				model.disableEmail(email, this, function (viewObject, data) {
					viewObject.findUserWithPhoneNumberInternal.call(viewObject);
				});
			}
			else
			{
				model.enableEmail(email, this, function (viewObject, data) {
					viewObject.findUserWithPhoneNumberInternal.call(viewObject);
				});
			}
		},

		unlinkIBSAccount:function(e)
		{
			e.preventDefault();

			var email = e.currentTarget.attributes.email.nodeValue;

			var unlinkIBSAccounButton = document.getElementById("buttonUnlinkIBSAccount" + email);
			unlinkIBSAccounButton.disabled = true;

			model.unlinkAccount(email, this, function (viewObject, data) {
				unlinkIBSAccounButton.innerText = TaxiHail.localize("IBS Account Unlinked");
			});
		}
	});
}());