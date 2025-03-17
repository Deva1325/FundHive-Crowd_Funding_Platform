$(document).ready(function () {
    console.log("Login JS Loaded");

    // Toggle Password Visibility
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

    // Login Form Validation & Submission
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
        errorPlacement: function (error, element) {
            error.insertAfter(element);
        },
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
                    console.log("Login Response:", result);

                    if (result.success) {
                        showToast("success", "Login Successful!");

                        setTimeout(() => {
                            if (result.redirectUrl) {
                                window.location.assign(result.redirectUrl);
                            } else {
                                showToast("error", "Redirect URL is missing!");
                            }
                        }, 1500);
                    } else {
                        showToast("error", result.message || "Login Failed! Please check your credentials.");
                    }
                },
                error: function () {
                    showToast("error", "An error occurred. Please try again.");
                },
                complete: function () {
                    btnLogin.prop("disabled", false);
                    btnLoader.addClass("d-none").removeClass("d-inline-block");
                }
            });
        }
    });

    // Forgot Password Form Validation & Submission
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
                email: "Enter a valid email address."
            }
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const formData = new FormData(form);

            $.ajax({
                url: '/Account/ForgotPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    showToast("success", result.message);
                },
                error: function () {
                    showToast("error", "An error occurred while sending the email.");
                }
            });
        }
    });

    // Reset Password Form Validation & Submission
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
                minlength: "Confirm password must be at least 8 characters.",
                equalTo: "Passwords do not match."
            }
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const formData = new FormData(form);

            $.ajax({
                url: '/Account/ResetPassword',
                type: 'POST',
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    showToast("success", result.message);
                    if (result.success) {
                        setTimeout(() => {
                            window.location.href = '/Account/Login';
                        }, 1500);
                    }
                },
                error: function () {
                    showToast("error", "An error occurred while resetting the password.");
                }
            });
        }
    });

    // Toast Message Function
    function showToast(message, type) {
        let bgColor = type === "success" ? "bg-success" : "bg-danger";
        let toastId = "toast-" + new Date().getTime();

        // Ensure the toast container exists
        if ($("#toastContainer").length === 0) {
            $("body").append('<div id="toastContainer" class="position-fixed top-0 end-0 p-3" style="z-index: 1050;"></div>');
        }

        $("#toastContainer").append(`
        <div id="${toastId}" class="toast align-items-center text-white ${bgColor} border-0 show" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `);

        let toastElement = new bootstrap.Toast(document.getElementById(toastId));
        toastElement.show();

        setTimeout(() => {
            $("#" + toastId).fadeOut("slow", function () { $(this).remove(); });
        }, 3000);
    }


});
