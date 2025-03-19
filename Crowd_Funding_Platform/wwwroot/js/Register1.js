$(document).ready(function () {

    const phoneInputField = document.querySelector("#PhoneNo");
    phoneInput = window.intlTelInput(phoneInputField, {
        initialCountry: "IN",
        geoIpLookup: function (callback) {
            fetch('https://ipapi.co/json', { mode: 'no-cors' })
                .then((response) => response.json())
                .then((data) => callback(data.country_code))
                .catch(() => callback("us"));
        },
        utilsScript: "https://cdnjs.cloudflare.com/ajax/libs/intl-tel-input/17.0.8/js/utils.js"
    });

    $("#Registration").validate({
        rules: {
            Username: {
                required: true,
                minlength: 4,
                maxlength: 50
            },
            Email: {
                required: true,
                email: true
            },
            PasswordHash: {
                required: true,
                minlength: 6
            },
            ConfirmPassword: {
                required: true,
                minlength: 6,
                equalTo: "#PasswordHash"
            },
            PhoneNumber: {
                required: true,
                minlength: 10,
                maxlength: 15,
                digits: true
            }
        },
        messages: {
            Username: {
                required: "Please enter a username.",
                minlength: "Username must be at least 4 characters.",
                maxlength: "Username cannot exceed 50 characters."
            },
            Email: {
                required: "Please enter your email.",
                email: "Please enter a valid email address."
            },
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 6 characters."
            },
            ConfirmPassword: {
                required: "Please confirm your password.",
                minlength: "Password confirmation must be at least 8 characters.",
                equalTo: "Passwords do not match."
            },
            PhoneNumber: {
                required: "Please enter your Phone NO.",
                minlength: "Phone No must be at least 10 digits.",
                digits: "Please enter only digits."
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault();

            const btnRegister = $("#btnRegister");
            const btnLoader = $("#btnLoader");

            btnRegister.prop("disabled", true);
            btnLoader.removeClass("d-none");

            const formData = new FormData(form);

            //$.ajax({
            //    url: '/Account/Registration',
            //    type: 'POST',
            //    processData: false,
            //    contentType: 'application/x-www-form-urlencoded; charset=UTF-8',
            //    data: formData,
            //    success: function (result) {
            //        if (result.success) {
            //            toastr.success(result.message);
            //            setTimeout(() => {
            //                window.location.href = '/Account/OtpCheck';
            //            }, 2000);
            //        } else {
            //            toastr.error(result.message);
            //        }
            //    },
            //    error: function () {
            //        toastr.error('An error occurred while registering the user.');
            //    }
            //});

            $.ajax({
                url: '/Account/Registration',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    if (result.success) {
                        toastr.success(result.message);
                        setTimeout(() => {
                            window.location.href = '/Account/OtpCheck';  // Redirect on success
                        }, 2000);
                    } else {
                        toastr.error(result.message);
                    }
                },
                complete: function () {
                    btnRegister.prop("disabled", false);
                    btnLoader.addClass("d-none");
                },

                error: function () {
                    toastr.error('An error occurred while registering the user.');
                }
            });
        }
    });

    // Toggle password visibility

    $('.toggle-password-btn').on('click', function () {
        const target = $(this).data('target');
        const input = $(target);
        const icon = $(this).find('i');

        const isPassword = input.attr('type') === 'password';
        input.attr('type', isPassword ? 'text' : 'password');
        icon.toggleClass('fa-eye fa-eye-slash');
    });

    //$('.toggle-password-btn').on('click', function () {
    //    const target = $(this).data('target');
    //    const input = $(target);
    //    const icon = $(this).find('i');

    //    if (input.attr('type') === 'password') {
    //        input.attr('type', 'text');
    //        icon.removeClass('fa-eye').addClass('fa-eye-slash');
    //    } else {
    //        input.attr('type', 'password');
    //        icon.removeClass('fa-eye-slash').addClass('fa-eye');
    //    }
    //});


    $('#PasswordHash').on('input', function () {
        const password = $(this).val();
        const strength = checkPasswordStrength(password);
        $('#password-strength').text(strength).css('color', strength === 'Strong' ? 'green' : 'red');
    });

    function checkPasswordStrength(password) {
        if (password.length < 6) {
            return 'Weak';
        }
        if (/[A-Z]/.test(password) && /[0-9]/.test(password) && /[!@#$%^&*]/.test(password)) {
            return 'Strong';
        }
        return 'Moderate';
    }

});
