//window.onload = () => {
//    if (typeof grecaptcha !== "undefined") {
//        console.log("✅ reCAPTCHA is loaded.");
//    } else {
//        console.error("❌ reCAPTCHA failed to load.");
//    }
//};

$(document).ready(function () {
    console.log("Login JS Loaded");

    // ================================
    // 🔥 Toastr Configuration
    // ================================
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "newestOnTop": true,
        "progressBar": true,
        "positionClass": "toast-top-right",
        "preventDuplicates": false,
        "showDuration": "300",
        "hideDuration": "1000",
        "timeOut": "5000",
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "fadeIn",
        "hideMethod": "fadeOut"
    };

    // Function to display Toastr messages
    function showToast(type, message) {
        if (type === "success") {
            toastr.success(message);
        } else if (type === "error") {
            toastr.error(message);
        } else if (type === "warning") {
            toastr.warning(message);
        } else {
            toastr.info(message);
        }
    }

    // ================================
    // 👁️ Toggle Password Visibility
    // ================================
    $('.toggle-password-btn').on('click', function () {
        const target = $(this).data('target'); // Get the target input field
        const input = $(target);
        const icon = $(this).find('i');

        if (input.attr('type') === 'password') {
            input.attr('type', 'text');
            icon.removeClass('fa-eye').addClass('fa-eye-slash');
        } else {
            input.attr('type', 'password');
            icon.removeClass('fa-eye-slash').addClass('fa-eye');
        }
    });

    // ================================
    // 🔥 Password Strength Indicator (Text-based Only)
    // ================================
    const passwordInput = $("#PasswordHash");
    const strengthText = $("<small id='password-strength-text' class='form-text mt-1'></small>").insertAfter(passwordInput);

    passwordInput.on("input", function () {
        const password = $(this).val();

        if (password.length > 0) {
            const strength = getPasswordStrength(password);

            let color, label;

            switch (strength) {
                case "Weak":
                    color = "#FF4C4C";  // Red
                    label = "Weak";
                    break;
                case "Fair":
                    color = "#FFB74D";  // Orange
                    label = "Fair";
                    break;
                case "Good":
                    color = "#64B5F6";  // Light Blue
                    label = "Good";
                    break;
                case "Strong":
                    color = "#4CAF50";  // Green
                    label = "Strong";
                    break;
                default:
                    color = "#BDBDBD";  // Grey
                    label = "Too Weak";
            }

            strengthText.text(`Password Strength: ${label}`)
                .css({
                    "color": color,
                    "font-weight": "bold",
                    "font-size": "14px"
                });

        } else {
            strengthText.text("").css("color", "");
        }
    });

    // Function to determine password strength
    function getPasswordStrength(password) {
        let strength = 0;

        if (password.length >= 8) strength++;
        if (/[a-z]/.test(password)) strength++;
        if (/[A-Z]/.test(password)) strength++;
        if (/[0-9]/.test(password)) strength++;
        if (/[\W]/.test(password)) strength++;

        if (strength < 2) return "Weak";
        if (strength < 3) return "Fair";
        if (strength < 4) return "Good";
        return "Strong";
    }

    // ================================
    // 🔑 Login Form Validation & Submission
    // ================================
    // ✅ Function to Reset reCAPTCHA on Expiry


    //function resetReCAPTCHA() {
    //    grecaptcha.reset();
    //}

    // ✅ Add reCAPTCHA token handling
    $("#loginForm").validate({
        errorClass: "text-danger fw-bold",
        rules: {
            EmailOrUsername: {
                required: true
            },
            Password: {
                required: true,
                minlength: 6
            }
        },
        messages: {
            EmailOrUsername: {
                required: "Please enter your email or username."
            },
            Password: {
                required: "Please enter your password.",
                minlength: "Password must be at least 6 characters."
            }
        },

        // onloadCallback: function () {
        //    grecaptcha.render('html_element', {
        //        'sitekey': '6LdlM_4qAAAAAD6vBYVZQXwbSJ6Uqh_FrH4va-_4'
        //    });
        //},

        submitHandler: function (form, event) {
            event.preventDefault();

            const btnLogin = $("#btnLogin");
            const btnLoader = $("#btnLoader");

            btnLogin.prop("disabled", true);
            btnLoader.removeClass("d-none").addClass("d-inline-block me-2");

            const formData = new FormData(form);

            $.ajax({
                url: "/Account/Login",
                type: "POST",
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    if (result.success) {
                        toastr.success("Login Successful!");

                        setTimeout(() => {
                            if (result.redirectUrl) {
                                window.location.href = result.redirectUrl;
                            } else {
                                toastr.error("Redirect URL is missing!");
                            }
                        }, 1500);

                    } else {
                        toastr.error(result.message || "Login Failed! Please check your credentials.");
                    }
                },
                error: function (xhr) {
                    console.error("Error:", xhr);
                    toastr.error("An error occurred. Please try again.");
                },

                complete: function () {
                    btnLogin.prop("disabled", false);
                    btnLoader.addClass("d-none").removeClass("d-inline-block");
                }

            });
        }
    });

    //    submitHandler: function (form, event) {
    //        event.preventDefault();

    //        const btnLogin = $("#btnLogin");
    //        const btnLoader = $("#btnLoader");

    //        btnLogin.prop("disabled", true);
    //        btnLoader.removeClass("d-none").addClass("d-inline-block me-2");



    //        //// ✅ Get reCAPTCHA v2 response token
    //        //const recaptchaResponse = grecaptcha.getResponse();

    //        //// ✅ Check if reCAPTCHA is completed
    //        //if (!recaptchaResponse) {
    //        //    toastr.error("Please complete the reCAPTCHA.");
    //        //    btnLogin.prop("disabled", false);
    //        //    btnLoader.addClass("d-none").removeClass("d-inline-block");
    //        //    return;
    //        //}

    //        const formData = new FormData(form);
    //        //formData.append('g-recaptcha-response', recaptchaResponse);

    //        $.ajax({
    //            url: "/Account/Login",
    //            type: "POST",
    //            processData: false,
    //            contentType: false,
    //            data: formData,
    //            success: function (result) {
    //                if (result.success) {
    //                    toastr.success("Login Successful!");

    //                    setTimeout(() => {
    //                        if (result.redirectUrl) {
    //                            window.location.href = result.redirectUrl;
    //                        } else {
    //                            toastr.error("Redirect URL is missing!");
    //                        }
    //                    }, 1500);
    //                } else {
    //                    toastr.error(result.message || "Login Failed! Please check your credentials.");

    //                    //// ✅ Reset reCAPTCHA if failed
    //                    //resetReCAPTCHA();
    //                }
    //            },
    //            error: function (xhr) {
    //                console.error("Error:", xhr);
    //                toastr.error("An error occurred. Please try again.");

    //                //// ✅ Reset reCAPTCHA on error
    //                //resetReCAPTCHA();
    //            },
    //            complete: function () {
    //                btnLogin.prop("disabled", false);
    //                btnLoader.addClass("d-none").removeClass("d-inline-block");
    //            }
    //        });
    //    }
    //});

       

 
    // ================================
    // 📧 Forgot Password Validation & Submission
    // ================================
    $("#forgotPassword").validate({
        rules: {
            Email: {
                required: true,
                email: true
            }
        },
        messages: {
            Email: {
                required: "Please enter your email.",
                email: "Enter a valid email address."
            }
        },
        errorPlacement: function (error, element) {
            error.insertAfter(element);
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const btnForgot = $("#btnForgot");
            const btnLoader = $("#btnLoaderForgot");

            btnForgot.prop("disabled", true);
            btnLoader.removeClass("d-none").addClass("d-inline-block me-2");

            const formData = new FormData(form);

            $.ajax({
                url: '/Account/ForgotPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    console.log("Forgot Password Response:", result);

                    if (result.success) {
                        showToast("success", result.message);
                    } else {
                        showToast("error", result.message || "Failed to send reset email.");
                    }
                },
                error: function () {
                    showToast("error", "An error occurred while sending the email.");
                },
                complete: function () {
                    btnForgot.prop("disabled", false);
                    btnLoader.addClass("d-none").removeClass("d-inline-block");
                }
            });
        }
    });

    $('.toggle-password-btn').on('click', function () {
        const target = $(this).data('target');
        const input = $(target);
        const icon = $(this).find('i');

        const isPassword = input.attr('type') === 'password';
        input.attr('type', isPassword ? 'text' : 'password');
        icon.toggleClass('fa-eye fa-eye-slash');
    });

    // ================================
    // 🔒 Reset Password Validation & Submission
    // ================================
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
            }
        },
        messages: {
            PasswordHash: {
                required: "Please enter a password.",
                minlength: "Password must be at least 8 characters."
            },
            ConfirmPassword: {
                required: "Please confirm your password.",
                equalTo: "Passwords do not match."
            }
        },
        errorPlacement: function (error, element) {
            error.insertAfter(element);
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const btnReset = $("#btnReset");
            const btnLoader = $("#btnLoaderForgot");

            btnReset.prop("disabled", true);
            btnLoader.removeClass("d-none").addClass("d-inline-block me-2");

            const formData = new FormData(form);

            $.ajax({
                url: '/Account/ResetPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    if (result.success) {
                        toastr.success("Password Changed Successfully");

                        setTimeout(() => {
                            window.location.href = '/Account/Login';
                        }, 1500);

                    } else {
                        toastr.error(result.message || "Failed to reset password.");
                    }
                    //if (result.success) {
                    //    toastr.success("Password Changed Successfully");
                    //    //showToast("success", result.message);
                    //    window.location.href = '/Account/Login';
                    //} else {
                    //    toastr.error("Failed to reset password.");
                       
                    //}
                },
                error: function () {
                    toastr.error("An error occurred while resetting the password.");
                },
                complete: function () {
                    btnReset.prop("disabled", false);
                    btnLoader.addClass("d-none").removeClass("d-inline-block");
                }
            });
        }
    });

});


//    // ================================
//    // 🔒 Reset Password Validation & Submission
//    // ================================
//    $("#ResetPassword").validate({
//        rules: {
//            PasswordHash: {
//                required: true,
//                minlength: 8
//            },
//            ConfirmPassword: {
//                required: true,
//                minlength: 8,
//                equalTo: "#PasswordHash"
//            }
//        },
//        messages: {
//            PasswordHash: {
//                required: "Please enter a password.",
//                minlength: "Password must be at least 8 characters."
//            },
//            ConfirmPassword: {
//                required: "Please confirm your password.",
//                minlength: "Confirm password must be at least 8 characters.",
//                equalTo: "Passwords do not match."
//            }
//        },
//        errorPlacement: function (error, element) {
//            error.insertAfter(element);
//        },
//        submitHandler: function (form, event) {
//            event.preventDefault();

//            const btnReset = $("#btnReset");
//            const btnLoader = $("#btnLoaderReset");

//            btnReset.prop("disabled", true);
//            btnLoader.removeClass("d-none").addClass("d-inline-block me-2");

//            const formData = new FormData(form);

//            $.ajax({
//                url: '/Account/ResetPassword',
//                type: 'POST',
//                processData: false,
//                contentType: false,
//                data: formData,
//                success: function (result) {
//                    console.log("Reset Password Response:", result);

//                    if (result.success) {
//                        showToast("success", result.message);

//                        setTimeout(() => {
//                            window.location.href = '/Account/Login';
//                        }, 1500);
//                    } else {
//                        showToast("error", result.message || "Failed to reset password.");
//                    }
//                },
//                error: function () {
//                    showToast("error", "An error occurred while resetting the password.");
//                },
//                complete: function () {
//                    btnReset.prop("disabled", false);
//                    btnLoader.addClass("d-none").removeClass("d-inline-block");
//                }
//            });
//        }
//    });

//});
