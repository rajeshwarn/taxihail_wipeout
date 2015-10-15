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
			'click [data-action=resetPassword]': 'resetPassword',
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
			model.getAccountWithID(this, function (viewObject, data)
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

			var account = model.getAccount();

			var emailText = this.$("#email").val();
			var countryCodeText = this.$("#countryСode").val();
			var phoneNumberText = this.$("#phoneNumber").val();

			if (emailText && emailText.length > 0 && countryCodeText && countryCodeText.length > 0 && phoneNumberText && phoneNumberText.length > 0)
			{
				var countryCodes = model.getCountryCodes();
				var countryCode = null;

				for (i = 0; i < countryCodes.length; i++)
				{
					if (countryCodes[i].CountryISOCode.Code == countryCodeText)
					{
						countryCode = countryCodes[i].CountryISOCode;
						break;
					}
				}

				var bookingSettingsRequest =
				{
					name: account.name,
					firstName: account.firstName,
					lastName: account.lastName,
					email: emailText,
					country: countryCode,
					phone: phoneNumberText,
					vehicleTypeId: account.vehicleTypeId,
					chargeTypeId: account.chargeTypeId,
					providerId: account.providerId,
					numberOfTaxi: account.numberOfTaxi,
					passengers: account.passengers,
					accountNumber: account.accountNumber,
					customerNumber: account.customerNumber,
					defaultTipPercent: account.defaultTipPercent,
					payBack: account.payBack
				};

				var accountUpdateRequest =
				{
					accountId: account.id,
					bookingSettingsRequest: bookingSettingsRequest
				};

				model.saveAccount(accountUpdateRequest, this, function (viewObject, data)
				{
					if (data.status == 400 || data.status == 401 || data.status == 403 || data.status == 404 || data.status == 405 || data.status == 406 || data.status == 407
						 || data.status == 408 || data.status == 409 || data.status == 410 || data.status == 411 || data.status == 412 || data.status == 413 || data.status == 414
						 || data.status == 415 || data.status == 416 || data.status == 417 || data.status == 426 || data.status == 500 || data.status == 501 || data.status == 502
						 || data.status == 503 || data.status == 504 || data.status == 505)
					{
						sendRef.innerText = TaxiHail.localize("Error during account update");
					}
					else
					{
						viewObject.refresh();
					}
				});
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Email, country code and phone number should not be empty'));
			}
		},

		sendConfirmationCodeSMS: function (e)
		{
			e.preventDefault();

			var account = model.getAccount();

			if (account && account.email && account.settings.country.code && account.settings.phone
				&& account.email.toString().length > 0 && account.settings.country.code.toString().length > 0 && account.settings.phone.toString().length > 0)
			{
				var sendRef = document.getElementById("refSendSMS" + account.id);
				sendRef.disabled = true;
				sendRef.classList.add("label-account-linkitem-disabled");

				model.sendConfirmationCodeSMS(this, function (viewObject, data)
				{
					if (data.status == 200)
					{
						sendRef.innerText = TaxiHail.localize("Code has been sent");
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

		resetPassword: function (e)
		{
			e.preventDefault();

			var account = model.getAccount();

			if (account && account.email && account.email.toString().length > 0)
			{
				var resetPasswordRef = document.getElementById("refResetPassword" + account.id);
				resetPasswordRef.disabled = true;
				resetPasswordRef.classList.add("label-account-linkitem-disabled");

				model.resetPassword(this, function (viewObject, data)
				{
					if (data.status == 400 || data.status == 401 || data.status == 403 || data.status == 404 || data.status == 405 || data.status == 406 || data.status == 407
						 || data.status == 408 || data.status == 409 || data.status == 410 || data.status == 411 || data.status == 412 || data.status == 413 || data.status == 414
						 || data.status == 415 || data.status == 416 || data.status == 417 || data.status == 426 || data.status == 500 || data.status == 501 || data.status == 502
						 || data.status == 503 || data.status == 504 || data.status == 505)
					{
						resetPasswordRef.disabled = false;
						resetPasswordRef.classList.remove("label-account-linkitem-disabled");
						viewObject.$('.errors').text(TaxiHail.localize('Error during password reset'));
					}
					else if (data.status == 200 || data.status == 201)
					{
						var response = JSON.parse(data.responseText);

						resetPasswordRef.innerText = "Password has been reset to: ";

						var refResetPasswordResponse = document.getElementById("refResetPasswordResponse" + account.id);
						refResetPasswordResponse.innerText = response;
						refResetPasswordResponse.classList.remove("invisible");
					}
				});
			}
			else
			{
				this.$('.errors').text(TaxiHail.localize('Email should not be empty'));
			}
		},

		enableDisableAccount: function (e)
		{
			e.preventDefault();

			var account = model.getAccount();

			if (account)
			{
				var enableDisableAccountRef = document.getElementById("refEnableDisableAccount" + account.id);
				enableDisableAccountRef.disabled = true;
				enableDisableAccountRef.classList.add("label-account-linkitem-disabled");

				if (!account.disabledByAdmin)
				{
					model.disableEmail(this, function (viewObject, data)
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
					model.enableEmail(this, function (viewObject, data)
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

			var account = model.getAccount();

			if (account)
			{
				var unlinkIBSAccounRef = document.getElementById("refUnlinkIBSAccount" + account.id);
				unlinkIBSAccounRef.disabled = true;
				unlinkIBSAccounRef.classList.add("label-account-linkitem-disabled");

				model.unlinkAccount(this, function (viewObject, data)
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

			var account = model.getAccount();

			TaxiHail.confirm({
				title: "Credit card removal",
				message: "Confirm removing all credit cards for user " + account.name
			}).on('ok', function ()
			{
				var deleteCreditCardsInfoRef = document.getElementById("refDeleteCreditCardsInfo" + account.id);
				deleteCreditCardsInfoRef.disabled = true;
				deleteCreditCardsInfoRef.classList.add("label-account-linkitem-disabled");

				model.deleteAccountCreditCards(this, function (viewObject, data)
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