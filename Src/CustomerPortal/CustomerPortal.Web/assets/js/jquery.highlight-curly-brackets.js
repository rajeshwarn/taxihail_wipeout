(function ($) {

    $.fn.highlightCurlyBrackets = function () {

        return this.each(function () {
            var text = $(this).html().replace(/{{[^}]+}}/g, "<i class=highlight>$&</i>");
            $(this).html(text);
        });
    };

}(jQuery));