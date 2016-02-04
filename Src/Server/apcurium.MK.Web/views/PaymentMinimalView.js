(function () {
    TaxiHail.PaymentMinimalView = TaxiHail.TemplatedView.extend({
        events: {
            'click [data-action=save]': 'saveTip',
            'change :input': 'onPropertyChanged'
        },

        render: function () {

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
                || TaxiHail.parameters.isCMTEnabled
                || TaxiHail.parameters.isRideLinqCMTEnabled;

            _.extend(data,
            {
                displayTipSelection: displayTipSelection,
                tipPercentages: tipPercentages,
            });

            this.$el.html(this.renderTemplate(data));

            return this;
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
