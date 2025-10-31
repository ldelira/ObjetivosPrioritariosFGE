$(document).ready(function () {
    
    //nav toggle
    $(".nav-toggle").click(function(){
    $(".side-panel").toggle();
});
    
    $(window).load(function () {
        $('.testi-slider').flexslider({
            controlNav: false,
            directionNav: true
        });
        $('.testi-slider-white').flexslider({
            controlNav: true,
            directionNav: false,
            animation:'slide'
        });
        //masonry blog home v1
        var $container = $('.mas-grid');
        $container.imagesLoaded(function () {
            $container.masonry({
                itemSelector: '.mas-item',
                columnWidth: '.col-sm-3'
            });
        });

    });
    wow = new WOW(
            {
                animateClass: 'animated',
                offset: 100,
                mobile: true
            }
    );
    wow.init();
//counters
    $('.counter').counterUp({
        delay: 10,
        time: 4000
    });
});
$(window).scroll(function() {
if ($(this).scrollTop() > 1){  
    $('.navbar').addClass("fixed");
  }
  else{
    $('.navbar').removeClass("fixed");
  }
});


