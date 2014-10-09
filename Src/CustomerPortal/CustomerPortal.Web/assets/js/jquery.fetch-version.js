(function ($) {

    $.fn.fetchVersion = function () {

        return this.each(function () {
            var $this = $(this),
                id = $this.data().key;
            $.getJSON('http://services.taxihail.com/' + id + '/api/app/info')
                .done(function (data) {
                    $this.text("P:" + data.version);
                }).fail(function () {
                    $this.text("P:" + "N/A");
                });
        });
    };
    
    $.fn.fetchVersionStaging = function () {

        return this.each(function () {
            var $this = $(this),
                id = $this.data().key;
            $.getJSON('http://staging.taxihail.com/' + id + '/api/app/info')
                .done(function (data) {
                    $this.text("S:" + data.version);
                }).fail(function () {
                    $this.text("S:" + "N/A");
                });
        });
    };

}(jQuery));