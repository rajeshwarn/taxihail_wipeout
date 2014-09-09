﻿(function () {

    var View = TaxiHail.BookAccountChargeView = TaxiHail.TemplatedView.extend({

        tagName: 'form',
        events: {
            'click [data-action=cancel]': 'cancel',
            'change [name=settings.accountNumber]': 'loadPrompts'
        },

        initialize: function() {
            _.bindAll(this, 'book', 'showErrors');
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
            this.$('.errors').empty();
            this.$('#btBook').button('loading');

            var $ul = this.$('ul');
            $ul.first().empty();

            this.model.fetchQuestions(accountNumber)
                .done(_.bind(function (data) {

                    this.$('#btBook').button('reset');
                    this.$('#title').show();

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

                    this.model.set('questionsAndAnswers', data.questions);

                    if (accountNumber && accountNumber != '') {
                        var settings = this.model.get('settings');
                        settings.accountNumber = accountNumber;
                        this.model.save('settings', settings);
                    }

                    this.refreshValidationRules(data.questions);

                }, this))
                .fail(_.bind(function (response) {

                    this.$('#btBook').button('reset');
                    this.$('#title').hide();

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
        refreshValidationRules: function (questions) {

            var questionRules = new Object();
            var questionMessages = new Object();

            for (var i = 0; i < questions.length; i++) {

                var question = questions[i];
                if (question.isEnabled &&
                    (question.isRequired || question.maxLength > 0)) {
                    var name = 'answer' + i;

                    var rule = new Object();
                    var message = new Object();
                    if (question.isRequired) {
                        rule.required = true;
                        message.required = 'the answer is required' ;
                    }
                    if (question.maxLength > 0) {
                        rule.maxlength = question.maxLength;
                    }

                    questionRules[name] = rule;
                    questionMessages[name] = message;
                }

            }

            this.validate({
                rules: questionRules,
                messages: questionMessages,
                submitHandler: this.book
            });
        },

        book: function (form) {

            var questions = this.model.get('questionsAndAnswers');
            if (!questions)
            {
                this.loadPrompts();
            }else
            {
                var answers = this.$("input[name^='answer']");
                for (var i = 0; i < answers.length; i++) {
                    var answer = answers[i];
                    var index = answer.getAttribute('data-id');
                    questions[index].answer = answer.value;
                }
                this.model.save({}, {
                    success: TaxiHail.postpone(function (model) {
                        // Wait for order to be created before redirecting to status
                        ga('send', 'event', 'button', 'click', 'book web', 0);
                        TaxiHail.app.navigate('status/' + model.id, { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
                    }, this),
                    error: this.showErrors
                });
            }
        },

        cancel: function (e) {
            e.preventDefault();
            this.model.destroyLocal();
            TaxiHail.app.navigate('', { trigger: true, replace: true /* Prevent user from coming back to this screen */ });
        },

        showErrors: function (model, result) {
            this.$(':submit').button('reset');

            if (result.responseText) {
                result = JSON.parse(result.responseText).responseStatus;
            }

            var $alert = $('<div class="alert alert-error" />');
            if (result.errorCode == "CreateOrder_RuleDisable") {
                $alert.append($('<div />').text(result.message));
            }
            else if (result.errorCode == "AccountCharge_InvalidAnswer") {
                $alert.append($('<div />').text(result.message));
            }
            else if (result.statusText) {
                $alert.append($('<div />').text(this.localize(result.statusText)));
            }
            _.each(result.errors, function (error) {
                $alert.append($('<div />').text(this.localize(error.statusText)));
            }, this);
            this.$('.errors').html($alert);
        },

        onPropertyChanged: function (e) {
            var $input = $(e.currentTarget),
                attr = $input.attr('name').split('.');

            if (attr.length > 1 && this.model.has(attr[0])) {
                this.model.get(attr[0])[attr[1]] = $input.val();
            } else {
                this.model.set(attr[0], $input.val());
            }
        },
    });

    _.extend(View.prototype, TaxiHail.ValidatedView);

}())