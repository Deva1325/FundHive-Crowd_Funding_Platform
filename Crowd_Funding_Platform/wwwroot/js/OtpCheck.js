$(document).ready(function () {

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

    $("#OtpCheck").submit(function (event) {
        event.preventDefault()

        // Collect OTP from individual boxes
        let otp = '';
        let allFilled = true;

        $(".otp-box").each(function () {
            if ($(this).val() === '') {
                allFilled = false;
            }
            otp += $(this).val();
        });

        if (!allFilled) {
            toastr.error("Please enter all 6 digits of the OTP.");
            return; // Stop the AJAX call
        }

        // Assign the combined OTP to the hidden field
        $("#hiddenOtpInput").val(otp);

        const formData = {
            Email: $('#Email').val(),
            Otp: $('#hiddenOtpInput').val()
        }

        console.log(formData.Email)
        console.log(formData.Otp)
        // AJAX submission
        $.ajax({
            url: '/Account/OtpCheck',
            type: 'POST',
            data: formData,
            success: function (result) {

                if (result.success) {
                    toastr.success("Login Successful!");

                    setTimeout(() => {                       
                            //toastr.success("'Otp Verified Successfully");
                            window.location.href = '/Account/Login';                     
                    }, 1500);

                } else {
                    toastr.error(result.message || "Unable to verify OTP.");
                }
   
            },
            error: function () {
                toastr.error(result.message || "Unable to verify OTP.");
                //alert('Unable to verify OTP');
            }
        });
    })

    const timerElement = $("#timer");
    const resendSection = $("#resend-section");
    const resendButton = $("#resend-btn");
    const duration = 60;
    let timeRemaining = duration;

    // Update the timer every second
    const interval = setInterval(() => {
        if (timeRemaining <= 0) {
            clearInterval(interval);
            timerElement.text("You can resend the OTP.");
            resendSection.show(); // Show the resend button when timer expires
        } else {
            timeRemaining--;
            const minutes = Math.floor(timeRemaining / 60).toString().padStart(2, "0");
            const seconds = (timeRemaining % 60).toString().padStart(2, "0");
            timerElement.text(`Time remaining: ${minutes}: ${seconds}`);
        }
    }, 1000);

    //  Resend OTP functionality
    resendButton.click(function (e) {
        e.preventDefault();
        $.ajax({
            url: "/Account/ResendOTP", // Adjust the route to your server's endpoint
            method: "GET",
            success: function (data) {
                if (data.success) {
                   // alert("OTP has been resent to your email.");
                    toastr.success("OTP has been resent to your email.");
                    timeRemaining = duration; // Reset the timer
                    resendSection.hide(); // Hide the resend button
                    timerElement.text("Time remaining: 01:00"); // Reset the timer text
                    setInterval(interval); // Restart the interval
                } else {
                    toastr.error("Failed to resend OTP. Please try again.");
                    //alert("Failed to resend OTP. Please try again.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error resending OTP:", error);
                toastr.error("An error occurred. Please try again.");
                //alert("An error occurred. Please try again.");
            }
        });
    });

});
