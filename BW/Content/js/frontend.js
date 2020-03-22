$(function() {
    var windowScroll = function(first) {
        var winScroll = $(window).scrollTop() >= 1,
            test = $('#page').innerHeight(),
            winH = $(window).height(),
            headerH = $('#header').height(),
            x = test - winH;
        $(window).scroll(function() {
            var headerH = $('#header').height();
            if ($(window).scrollTop() >= 1 && x >= headerH || typeof first != 'undefined') {
                $('#header').addClass('fixed');
                $('#admin').addClass('fixed').css('top', headerH);
            }
            if ($(window).scrollTop() <= 0) {
                $('#header').removeClass('fixed');
                $('#admin').removeClass('fixed').css('top', 0);
            }
        });
    };
    windowScroll(true);
    ellipsis();
    $(window).resize(function() {
        ellipsis();
    });
});
var ellipsis = function() {
    var win_w = $(window).width();
    if (win_w < 768) {
        $('.ellipsis').ellipsis({
            row: 2
        });
    }
};