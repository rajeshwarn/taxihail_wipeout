(function () {
    var canCancel = true;
    TaxiHail.BookingStatusView = TaxiHail.TemplatedView.extend({

        className: 'booking-status-view',

        events: {
            'click [data-action=cancel]': 'cancel',
            'click #callDriverButton': 'callDriver',
            'click #messageDriverButton': 'messageDriver'
        },

        initialize: function() {

            var status = this.model.getStatus();
            this.interval = window.setInterval(_.bind(function() {
                this.fetch();
            }, status), 5000);
            
            // Variable use to store the reference to window.timeout
            // after an order has timed out
            this.redirectTimeout = null;
            
            status.on('change:ibsStatusId', this.render, this);
            status.on('change:ibsStatusId change:vehicleLatitude', this.onVehicleAssignedAndPositionUpdated, this);
            status.on('change:ibsStatusId', this.onStatusChanged, this);
            status.on('ibs:timeout', this.ontimeout, this);
            status.on('change:status', this.onTHStatusChanged, this);
        },

        onVehicleAssignedAndPositionUpdated: function (model, status) {
            if (model.showEta()) {
                TaxiHail.directionInfo.getAssignedEta(model.get('orderId'), model.get('vehicleLatitude'), model.get('vehicleLongitude')).done(
                    _.bind(function(result) {
                        var $eta = this.$('.eta');
                        if (result.duration) {
                            var formattedEta = TaxiHail.formatAssignedEta(result.duration, result.formattedDistance);

                            $eta.removeClass('hidden')
                                .find('#etaValue')
                                .text(formattedEta);
                        } else {
                            this.$('.eta').addClass('hidden');
                        }
                    }, this));
            } else {
                this.$('.eta').addClass('hidden');
            }
        },

        render: function() {
            var status = this.model.getStatus(),
                data = _.extend(status.toJSON(), {
                    isActive: status.isActive(),
                    callNumber: TaxiHail.parameters.defaultPhoneNumber,
                    showOrderNumber: TaxiHail.parameters.showOrderNumber
                });

            
            // Close popover if it is open
            // Otherwise it will stay there forever
            this.$('[data-action=call]').popover('hide');
            
            if(!data.ibsStatusDescription)
            {
                data.ibsStatusDescription = this.localize('Processing');
            }

            this.$el.html(this.renderTemplate(data));
            if (TaxiHail.parameters.hideDispatchButton == true) {
                this.$('#callDispatchButton').addClass('hidden');
            }
            
            var status = this.model.getStatus();
            var ibsStatusId = status.get('ibsStatusId');

            if (ibsStatusId == "wosLOADED") {
                this.$('#callDriverButton').addClass('hidden');
                this.$('#messageDriverButton').addClass('hidden');
            }

            if (TaxiHail.parameters.showCallDriver == true) {
                var driverInfos = status.get('driverInfos');
                if (driverInfos !== undefined) {
                    if (driverInfos.mobilePhone) {
                        this.$('#callDispatchButton').addClass('hidden');
                        this.$('#callDriverButton').removeClass('hidden');
                    }
                }
            }

            if (TaxiHail.parameters.showMessageDriver == true) {
                var driverInfos = status.get('driverInfos');
                if (driverInfos !== undefined) {
                    this.$('#messageDriverButton').removeClass('hidden');
                }
            }

            if (!status.isActive()) {
                this.$('[data-action=cancel]').addClass('disabled');
                canCancel = false;
            } else {
                canCancel = true;
            }

            this.$('[data-action=call]').popover({
                title: this.localize("Call us at"),
                content: TaxiHail.parameters.defaultPhoneNumber
            });

            return this;
        },
        
        remove: function() {

            // Close popover if it is open
            // Otherwise it will stay there forever
            this.$('[data-action=call]').popover('hide');

            this.$el.remove();

            // Stop polling for Order Status updates
            window.clearInterval(this.interval);

            window.clearTimeout(this.redirectTimeout);

        },

        cancel: function(e) {
            e.preventDefault();
            if (canCancel == true) {

                var confirmationMessage = this.localize('modal.cancelOrder.message');
                if (this.model.getStatus().warnForCancellationFees()) {
                    confirmationMessage = this.localize('modal.cancelOrder.message.warnForFees').format(TaxiHail.parameters.applicationName);
                }

                TaxiHail.confirm({
                    title: this.localize('Cancel Order'),
                    message: confirmationMessage,
                    cancelButton: this.localize('modal.cancelOrder.cancelButton')
                }).on('ok', function () {
                    this.model.cancel().done(function () {
                        TaxiHail.orderService.clearCurrentOrder();

                        // Redirect to Home
                        TaxiHail.app.navigate('', { trigger: true });
                    });
                }, this);
            }
        },

        callDriver: function (e) {
            e.preventDefault();
            
            TaxiHail.confirm({
                title: this.localize('Call Driver'),
                message: this.localize('modal.callDriver.message'),
                cancelButton: this.localize('modal.callDriver.cancelButton')
            }).on('ok', function () {
                this.model.initiateCallToDriver()
                    .done(_.bind(function (result) {
                        if (result) {
                            TaxiHail.message({
                                title: this.localize('Call Driver'),
                                message: this.localize('modal.callDriver.success')
                            });
                        } else {
                            TaxiHail.message({
                                title: this.localize('Error'),
                                message: this.localize('modal.callDriver.error')
                            });
                        }
                    }, this));
            }, this);
        },

        messageDriver: function (e) {
            e.preventDefault();

            var message = prompt(this.localize('modal.messageDriver.message'), "");
            if (message != null) {
                var vehicleNumber = this.model.getStatus().get('vehicleNumber');
                this.model.sendMessageToDriver(vehicleNumber, message)
                    .done(_.bind(function (result) {
                        if (result) {
                            TaxiHail.message({
                                title: this.localize('Message Driver'),
                                message: this.localize('modal.messageDriver.success')
                            });
                        } else {
                            TaxiHail.message({
                                title: this.localize('Error'),
                                message: this.localize('modal.messageDriver.error')
                            });
                        }
                    }, this));
            }
        },
        
        onStatusChanged: function (model, status) {
            if(model.isCompleted()){

                // Prevent further updated
                window.clearInterval(this.interval);
                
                var message = this.localize('ThankYouNoteFormat').replace('{{ApplicationName}}', TaxiHail.parameters.applicationName);
                var options = {
                    title: this.localize('Ride Complete'),
                    message: message,
                    autoHide: false
                };

                if (model.canSendReceipt()) {
                    options.cancelButton = this.localize('Done');
                    options.confirmButton = this.localize('Send Receipt');
                } else {
                    options.cancelButton = null; // Hide Cancel button
                    options.confirmButton = this.localize('Done');
                }

                TaxiHail.confirm(options)
                    .on('cancel', function(view) {
                        view.hide();
                    }, this).on('ok', function(view) {

                        if(model.canSendReceipt()) {
                            // display "Sending..." for a brief delay before redirecting to home
                            var $button = view.$('[data-action=confirm]')
                                .data('loadingText', this.localize('Sending...'))
                                .button('loading');
                            
                            TaxiHail.postpone(function() {
                                this.model.sendReceipt()
                                    .done(function(){
                                            view.hide();
                                            TaxiHail.app.navigate('', {trigger: true});
                                        })
                                    .fail(_.bind(function(){
                                        $button.text(this.localize('Cannot Send Receipt'));
                                    }, this));
                                }, this).call();
                        } else {
                            view.hide();
                            TaxiHail.app.navigate('', {trigger: true});
                        }

                    }, this);
            }
        },

        onTHStatusChanged: function (model, status) {
            if (status.toUpperCase() === 'TIMEDOUT' && this.model._status.get('nextDispatchCompanyKey') != "") {

                var alwaysAccept = $.cookie('THNetwork_always_accept');

                if ((alwaysAccept && alwaysAccept === 'true')
                    || TaxiHail.parameters.autoConfirmFleetChange) {
                    this.switchDispatchCompany(this.model);
                } else {
                    TaxiHail.confirm({
                        title: this.localize('NetworkTimeOutPopupTitle'),
                        message: this.localize('NetworkTimeOutPopupMessage').format(this.model._status.get('nextDispatchCompanyName')),
                        confirmButton: this.localize('NetworkTimeOutPopupAccept'),
                        alwaysButton: this.localize('NetworkTimeOutPopupAlways'),
                        cancelButton: this.localize('NetworkTimeOutPopupRefuse')
                    }).on('ok', function () {
                        this.switchDispatchCompany(this.model);
                    }, this)
                    .on('always', function () {
                        $.cookie('THNetwork_always_accept', 'true', { expires: 20 * 365 }); // "Never" expires

                        this.switchDispatchCompany(this.model);
                    }, this)
                    .on('cancel', function () {
                        if (this.model._status.get('status').toUpperCase() === 'TIMEDOUT') {
                            this.model.ignoreDispatchCompanySwitch();
                        }
                    }, this);
                }
            }
        },

        switchDispatchCompany: function(model)
        {
            if (model._status.get('status').toUpperCase() === 'TIMEDOUT') {
                try {
                    model._status.set('ibsStatusDescription',
                            this.localize('NetworkContactingNextDispatchDescription').format(model._status.get('nextDispatchCompanyName')));

                    // Refresh the status description
                    this.render();

                    var call = model.switchOrderToNextDispatchCompany();
                    call.success(function (response) {
                        model.set('status', response.status);
                    });

                } catch (ex) {
                    TaxiHail.confirm({
                        title: this.localize('NetworkFleetDispatchErrorTitle'),
                        message: ex
                    });
                }
            }
        },

        ontimeout: function(model, status) {
            this.redirectTimeout = window.setTimeout(function() {
                TaxiHail.app.navigate('', { trigger: true });
            }, 10 * 1000);
        }

    });
}());