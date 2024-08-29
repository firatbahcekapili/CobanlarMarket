$(document).ready(function () {
    (function ($) {
        $.fn.invisible = function () {
            return this.each(function () {
                $(this).css("visibility", "hidden");
                $(this).css({ opacity: 0 });
            });
        };
        $.fn.visible = function () {
            return this.each(function () {
                $(this).css("visibility", "visible");
                $(this).css({ opacity: 1 });
            });
        };
    }(jQuery));
});



$(".search-button").on("click", function () {
    $('.modal-search').visible();
});
$(".close-search").on("click", function () {
    $('.modal-search').invisible()
});
