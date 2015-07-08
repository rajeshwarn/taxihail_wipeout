(function ($) {

    $.fn.fetchStatus = function (onlyGetActive) {

        return this.each(function () {
            var $this = $(this),
                id = $this.data().key,
                status = $this.data().status;
                if (!onlyGetActive || (status != "Error" && status != "Success" && status != "Cancelled")) {
                    $.get('/Admin/Deployment/StatusText/' + id, function (result) {
                        $this.html(result);
                    });

                    $.get('/Admin/Deployment/DetailsText/' + id, function (result) {

                        $this.prop('title', result);
                    });
                } 
        });
    };
    
    

}(jQuery));