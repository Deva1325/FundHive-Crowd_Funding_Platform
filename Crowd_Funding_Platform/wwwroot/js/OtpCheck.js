$(document).ready(function () {
    $("#OtpCheck").submit(function (event) {
        event.preventDefault()

        const formData = {
            Email: $('#Email').val(),
            Otp: $('#Otp').val()
        }

        console.log(formData.Email)
        console.log(formData.Otp)
        // AJAX submission
        $.ajax({
            url: '/Account/OtpCheck',
            type: 'POST',
            data: formData,
            success: function (result) {
                alert(result.message);
                if (result.success) {
                    
                    alert('Otp Verified Successfully');
                    window.location.href = '/Account/Login';
                }
            },
            error: function () {
                alert('Unable to verify OTP');
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
                    alert("OTP has been resent to your email.");
                    timeRemaining = duration; // Reset the timer
                    resendSection.hide(); // Hide the resend button
                    timerElement.text("Time remaining: 01:00"); // Reset the timer text
                    setInterval(interval); // Restart the interval
                } else {
                    alert("Failed to resend OTP. Please try again.");
                }
            },
            error: function (xhr, status, error) {
                console.error("Error resending OTP:", error);
                alert("An error occurred. Please try again.");
            }
        });
    });

});
