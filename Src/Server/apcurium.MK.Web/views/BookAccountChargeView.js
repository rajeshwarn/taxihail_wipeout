(function () {

    TaxiHail.BookAccountChargeView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        events: {
            'click [data-action=loadPrompts]': 'loadPrompts',
        },
        render: function () {
            var html = this.renderTemplate(this.model.toJSON());
            this.$el.html(html);
            var settings = this.model.get('settings');
            if (settings.accountNumber
                && settings.accountNumber != '')
            {
                this.refreshPrompts(settings.accountNumber);
            }
            return this;
        },
        refreshPrompts: function(accountNumber)
        {
            this.$('#btloadPrompts').button('loading');

            var $ul = this.$('ul');
            $ul.first().empty();

            this.model.fetchQuestions(accountNumber)
                .done(_.bind(function (data) {

                    //this.refreshValidation(data.questions);

                    this.$('#btloadPrompts').button('reset');

                    var $ul = this.$('ul');

                    var items = data.questions.reduce(function (memo, model) {
                        if (model.question
                            && model.question != '') {
                            memo.push(new TaxiHail.QuestionItemView({
                                model: model
                            }).render().el);
                        }
                        return memo;
                    }, []);

                    $ul.first().append(items);


                }, this))
                .fail(_.bind(function (response) {

                    this.$('#btloadPrompts').button('reset');

                    var alert = new TaxiHail.AlertView({
                        message: TaxiHail.localize(response.statusText),
                        type: 'error'
                    });
                    alert.on('ok', alert.remove, alert);
                    this.$('.errors').html(alert.render().el);

                }, this));
        },
        loadPrompts: function()
        {
            var accountNumber = $('#inputAccountNumber').val();
            if(accountNumber)
            {
                this.refreshPrompts(accountNumber);
            }
        },
        refreshValidation: function(questions)
        {
            
            this.validate({
                rules: {
                    'settings.name': "required",
                    'settings.phone': {
                        tenOrMoreDigits: true,
                        minlength: 10
                    },
                    'settings.passengers': {
                        required: true,
                        number: true
                    },
                    'settings.largeBags': {
                        number: true
                    }
                },
                messages: {
                    'settings.name': {
                        required: TaxiHail.localize('error.NameRequired')
                    },
                    'settings.phone': {
                        required: TaxiHail.localize('error.PhoneRequired'),
                        tenOrMoreDigits: TaxiHail.localize('error.PhoneBadFormat')
                    },
                    'settings.passengers': {
                        required: TaxiHail.localize('error.PassengersRequired'),
                        number: TaxiHail.localize('error.NotANumber')
                    },
                    'settings.largeBags': {
                        number: TaxiHail.localize('error.NotANumber')
                    }
                },
                submitHandler: this.book
            });
        },
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}())