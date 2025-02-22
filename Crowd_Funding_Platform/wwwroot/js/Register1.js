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
            UserName: {
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
                minlength: 8,
                equalTo: "#PasswordHash"
            },
        },
        messages: {
            Username: {
                required: "Please enter a username.",
                minlength: "Username must be at least 3 characters.",
                maxlength: "Username cannot exceed 15 characters."
            },
            //FirstName: {
            //    required: "Please enter your first name."
            //},
            Email: {
                required: "Please enter your email.",
                email: "Please enter a valid email address."
            },
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required: "Please enter confirm password",
                minlength: "Confirm password must be least 8 characters",
                equalTo: "Password doesn't match"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault();
            const formData = new FormData(form);

            $.ajax({
                url: '/Account/Registration',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        alert('Successfull');
                        window.location.href = '/Account/OtpCheck';
                    }
                },
                error: function () {
                    alert('An error occured while registering the user.');
                }
            });
        }

    });

    // Toggle password visibility
    $('.toggle-password-btn').on('click', function () {
        const target = $(this).data('target'); // Get the target input field
        const input = $(target);
        const icon = $(this).find('i');

        // Toggle between password and text
        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash'); // Change icon
        } else {
            input.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye'); // Change icon
        }
    });

    $.validator.addMethod("lettersOnly", function (value, element) {
        return this.optional(element) || /^[a-zA-Z]+$/.test(value);
    }
    )
});
