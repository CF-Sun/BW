$(function() {
    $('.slider-banner').slick({
        autoplay: true,
        autoplaySpeed: 2000,
        lazyLoad: 'ondemand',
        dots: true,
        arrows: false,
        infinite: true,
        speed: 1000,
        fade: true,
        cssEase: 'linear'
    });
});