(function () {

	TaxiHail.AccountsManagementView = TaxiHail.TemplatedView.extend({

		model: null, //AccountsManagementModel.js

		tagName: 'form',
		className: 'form-horizontal',

		events:
		{
			'click [data-action=findUserWithSearchCriteria]': 'findUserWithSearchCriteria',
			'keyup [data-action=findUserWithSearchCriteriaEnterOnTextfield]': 'findUserWithSearchCriteria',
			'click [data-action=goToAccountPage]': 'goToAccountPage',
		},

		initialize: function (accountsManagementModel)
		{
			model = accountsManagementModel;
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

		goToAccountPage: function (e)
		{
			e.preventDefault();
			model.goToAccountManagement(e.currentTarget.attributes.accid.nodeValue);
		}
	});
}());