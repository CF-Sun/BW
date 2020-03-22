(function($) {
    var dec_height = function() {
        var content = $('#page').height(),
            win_h = $(window).height(),
            main_h = $('#main').height(),
            need_h = win_h - content;
        if (content < win_h) {
            $('#main').css("min-height", main_h + need_h);
        }
    };
    var menu_mt = function() {
        var winW = $(window).width();
        if (winW < 992) {
            var headerH = $('#header').innerHeight();
            $('.offcanvas-collapse').css('top', headerH);
        }
    };
    $(window).on('load', function() {
        var winW = $(window).width();
        $('#loading').fadeOut(1000);
        dec_height();
    });
    $(window).resize(function() {
        var winW = $(window).width();
        dec_height();
    });
    var burgerMenu = function() {
        $('.menu').click(function() {
            $('.offcanvas-collapse').toggleClass('open');
            if ($('.offcanvas-collapse').hasClass('open')) {
                $('.burger').addClass('active');
                $('body').addClass("no-scroll");
                $('.SideBar').on('click', function() {
                    // open sidebar
                    $('.offcanvas-collapse').removeClass('open');
                    $('.burger').removeClass('active');
                    // fade in the overlay
                    $('.overlay').fadeIn();
                    $('.collapse.in').toggleClass('in');
                    $('a[aria-expanded=true]').attr('aria-expanded', 'false');
                });
            } else {
                $('.burger').removeClass('active');
                $('body').removeClass("no-scroll");
                $('.sub-menu').removeClass('active');
            }
        });
    };
    var windowScroll = function() {
        var winScroll = $(window).scrollTop() >= 1,
            test = $('#page').innerHeight(),
            winH = $(window).height(),
            headerH = $('#header').height(),
            x = test - winH;
        $(window).scroll(function () {
            var headerH = $('#header').height();
            if ($(window).scrollTop() >= 1 && x >= headerH) {
                $('#header').addClass('fixed');
                $('#admin').addClass('fixed').css('top', headerH);
                $('#progress').addClass('fixed').css('top', headerH + 25);
                menu_mt();
            }
            if ($(window).scrollTop() <= 0) {
                $('#header').removeClass('fixed');
                $('#admin').removeClass('fixed').css('top', 0);
                $('#progress').removeClass('fixed').css('top', 25);
                menu_mt();
            }
        });
    };
    var hover = function() {
        var $win = $(window).width();
        if ($win > 992) {
            $(".sub-menu-parent,.sub-menu-parent2").hover(function () {
                //Add a class of current and fade in the sub-menu
                $(this).children(".sub-menu").css({
                    'min-width': '10rem',
                    'display': 'block',
                    'opacity': '1',
                    'z-index': '1',
                    'transform': 'translateY(0%)'
                });
            }, function () {
                $(this).find(".sub-menu").css('display', 'none');
            });
        }
        if ($win < 992) {
            var headerH = $('#header').height();
            $(".sub-menu-parent").click(function() {
                if ($(this).find('.sub-menu').hasClass('active')) {
                    $(this).find('.sub-menu').removeClass('active');
                } else {
                    $('.sub-menu.active').removeClass('active');
                    $(this).find('.sub-menu').addClass('active');
                }
            });
            $("li.href").click(function() {
                $('.burger').removeClass('active');
                $('body').removeClass("no-scroll");
                $('.sub-menu').removeClass('active');
                $('.offcanvas-collapse').removeClass('open');
            });
        }
    };
    var goToTop = function() {
        $('.js-gotop').on('click', function(event) {
            event.preventDefault();
            $('html, body').animate({
                scrollTop: $('html').offset().top
            }, 500, 'easeInOutExpo');
            return false;
        });
        $(window).scroll(function() {
            var $win = $(window);
            if ($win.scrollTop() > 200) {
                $('.js-top').addClass('active');
            } else {
                $('.js-top').removeClass('active');
            }
        });
    };

    $(function() {
        menu_mt();
        burgerMenu();
        new WOW().init();
        windowScroll();
        hover();
        goToTop();
    });
})(jQuery)