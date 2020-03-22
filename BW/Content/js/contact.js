$(function() {
    $(document).on('show.bs.modal', '.modal', function (event) {
        var zIndex = 1040 + (10 * $('.modal:visible').length);
        $(this).css('z-index', zIndex);
        setTimeout(function() {
            $('.modal-backdrop').not('.modal-stack').css('z-index', zIndex - 1).addClass('modal-stack');
        }, 0);
    });
    $('.custom-file-input').on('change', function () {
        //get the file name
        var fileName = $(this).val();
        //replace the "Choose a file" label
        $(this).next('.custom-file-label').html(fileName);
    });
    var contactForm = $('.confirm');
    function checkname(value) {
        if (value.length > 0) {
            var pattSimbo = /^[\u4e00-\u9fa5a-zA-Z\-_\s]{1,}$/;
            return !pattSimbo.test(value);
        } else {
            return false;
        }
    }
    contactForm.validator({
        custom: {
            'checkname': function($el) {
                return checkname($el.val());
            }
        }
    });
    contactForm.on('submit', function(e) {
        window.setTimeout(function() {
            var errors = $('.has-error');
            var position = $('.scrolltop').offset();
            var header = $('#header').outerHeight();
            var y = position.top - header + 'px';
            if (errors.length) {
                $('html, body').animate({ scrollTop: y }, 500);
            }
        }, 0);
        // if the validator does not prevent form submit
        if (!e.isDefaultPrevented()) {
            var url = "includes/contacts.php";

            // POST values in the background the the script URL
            $.ajax({
                type: "POST",
                url: url,
                data: $(this).serialize(),
                success: function(data) {
                    // data = JSON object that contact.php returns
                    var result = JSON.parse(data);
                    console.log(result);

                    // we recieve the type of the message: success x danger and apply it to the
                    var messageAlert = 'alert-' + result.type;
                    var messageText = result.message;

                    // let's compose Bootstrap alert box HTML
                    var alertBox = '<div class="alert ' + messageAlert + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + messageText + '</div>';

                    // If we have messageAlert and messageText
                    if (messageAlert && messageText) {
                        // inject the alert to .messages div in our form
                        $('.messages').html(alertBox);
                        // empty the form
                        contactForm[0].reset();
                    }
                }
            });
            return false;
        }
    });
});
var loadMore = function() {
    $("#contact-form .file-wrapper .loadMore").on('click', function(e) {
        e.preventDefault();
        $("#contact-form .upload:hidden").slice(0, 1).slideDown();
        if ($("#contact-form .upload:hidden").length == 0) {
            $("#contact-form .file-wrapper .loadMore").fadeOut('fast');
        }
    });
};