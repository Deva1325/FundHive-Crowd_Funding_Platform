$(document).ready(function () {
    console.log("regeger");
    //document.querySelectorAll('toggle-password-btn').forEach(toggle => {
    //    toggle.addEventListener('click', () => {
    //        const passwordInput = toggle.closest('.pass-group').querySelector('pass-input');

    //        // Toggle input type between "password" and "text"
    //        if (passwordInput.type === "password") {
    //            passwordInput.type = "text";
    //            toggle.classList.remove('fa-eye-slash');
    //            toggle.classList.add('fa-eye');
    //        } else {
    //            passwordInput.type = "password";
    //            toggle.classList.remove('fa-eye');
    //            toggle.classList.add('fa-eye-slash');
    //        }
    //    });
    //});


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

    $("#loginForm").validate({
        rules: {
            EmailOrUsername: {
                required: true,
            },
            Password: {
                required: true,
            }
        },
        messages: {
            EmailOrUsername: {
                required: "Please enter your Credentials."
            },
            Password: {
                required: "Please enter your Password.",
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            // AJAX submission
            $.ajax({
                url: '/Account/Login',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,

                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/Dashboard/Dashboard';
                       // alert('Login successful');
                    }
                },

                error: function () {
                    alert('An error occurred while Login.');
                }
            });
        }
    });

    $("#forgotPassword").validate({
        rules: {
            Email: {
                required: true,
                email: true
            }
        },
        messages: {
            Email: {
                required: "Please enter your Email.",
                email: "Not a valid email"
            }
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);

            // AJAX submission
            $.ajax({
                url: '/Account/ForgotPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,

                success: function (result) {
                    alert(result.message);
                },

                error: function () {
                    alert('An error occurred while sending the email.');
                }
            });

        }

    });

    $("#ResetPassword").validate({
        rules: {
            PasswordHash: {
                required: true,
                minlength: 8
            },
            ConfirmPassword: {
                required: true,
                minlength: 8,
                equalTo: "#PasswordHash"
            },
        },
        messages: {
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required: "Please enter confirm password",
                minlength: "Confirm password must be least 8 characters",
                equalTo: "Password doesn't match"
            },
        },

        submitHandler: function (form, event) {
            event.preventDefault()
            const formData = new FormData(form);
            //var formData = {
            //    PasswordHash: $('#PasswordHash').val(),
            //}
            // AJAX submission
            $.ajax({
                url: '/Account/ResetPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    alert(result.message);
                    if (result.success) {
                        window.location.href = '/Account/Login';
                    }
                },
                error: function () {
                    alert('An error occurred while registering the user.');
                }
            });
        }
    });

});