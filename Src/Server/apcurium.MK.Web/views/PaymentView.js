(function () {
    TaxiHail.PaymentView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=add]': 'add',
            'click [data-action=save]': 'saveTip',
            'change :input': 'onPropertyChanged'
        },
        initialize: function () {
            this.collection.on('selected', function (model, collection) {
                model.set("settings", this.model.get('settings'));
                var detailsView = this._detailsView = new TaxiHail.PaymentDetailView({
                    model: model,
                    parent: this
                });
                detailsView.on('cancel', this.render, this);
                this.$el.html(detailsView.render().el);
            }, this);
            this.collection.on('destroy cancel', this.render, this);
        },

        render: function () {

            if (this._detailsView) {
                this._detailsView.remove();
                this._detailsView = null;
            }
            var data = this.model.toJSON();

            var tipPercentages = [
             { id: 0, display: "0%" },
             { id: 5, display: "5%" },
             { id: 10, display: "10%" },
             { id: 15, display: "15%" },
             { id: 18, display: "18%" },
             { id: 20, display: "20%" },
             { id: 25, display: "25%" }
            ];

            if (!data.defaultTipPercent) {
                _.extend(data,
                {
                    defaultTipPercent: TaxiHail.parameters.defaultTipPercentage,
                });
            }

            var displayTipSelection = TaxiHail.parameters.isChargeAccountPaymentEnabled
                || TaxiHail.parameters.isPayPalEnabled
                || TaxiHail.parameters.isBraintreePrepaidEnabled
                || TaxiHail.parameters.isCMTPrepaidEnabled
                || TaxiHail.parameters.isRideLinqCMTPrepaidEnabled;

            _.extend(data,
            {
                displayTipSelection: displayTipSelection,
                tipPercentages: tipPercentages,
            });
            if (this.collection.models.length < TaxiHail.parameters.maxNumberOfCreditCards) {
                _.extend(data,
                   {
                       canAddCard: true
                   });
            }
            this.$el.html(this.renderTemplate(data));

            if (this.collection.length) {
                this.collection.each(this.renderItem, this);
               
            } else {
                this.$el.append($('<div>').addClass('no-result').text(TaxiHail.localize('order.no-result')));
            }

            return this;
        },

        remove: function () {
            this._detailsView && this._detailsView.remove();
        },

        add: function (e) {
            var creditCardInfo = new TaxiHail.CreditCard();

            this.view = this._detailsView = new TaxiHail.PaymentDetailView({
                model: creditCardInfo,
                parent: this
            });

            this.view.on('cancel', this.render, this);
            this.view.render();
            this.$el.html(this.view.el);
        },

        renderItem: function (model) {
            if (this.model.get('defaultCreditCard')) {
                model.set("isDefault", model.get('creditCardId') === this.model.get('defaultCreditCard').creditCardId);
            }

            model.set("numberOfCrecitCards", this.collection.models.length);
            var view = new TaxiHail.PaymentItemView({
                model: model
        });
            view.render();
            this.$('ul').append(view.el);
        },

        onPropertyChanged: function (e) {

            var dataNodeName = e.currentTarget.nodeName.toLowerCase();
            var elementName = e.currentTarget.name;

            var $input = $(e.currentTarget);
            var settings = this.model.get('settings');

            if (dataNodeName == "select") {
                var name = $input.attr("name");
                var value = $input.val();

                // Update local model values
                if (name === "defaultTipPercent") {
                    this.model.set("defaultTipPercent", value);
                    settings["defaultTipPercent"] = this.model.get("defaultTipPercent");
                    settings["email"] = this.model.get("email");
                }
            }

            this.$(':submit').removeClass('disabled');
        },

        saveTip: function () {
            // Update settings
            this.model.updateSettings()
                .fail(_.bind(function (result) {
                    this.$(':submit').button('reset');

                    var message = "";

                    if (result.statusText) {
                        message = result.statusText;
                    }
                    else {
                        message = TaxiHail.localize("error.accountUpdate");
                    }

                    var alert = new TaxiHail.AlertView({
                        message: message,
                        type: 'error'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.errors').html(alert.render().el);
                }, this));
        }
    });

}());
