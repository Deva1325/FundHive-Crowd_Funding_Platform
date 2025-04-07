$(document).ready(function () {
    console.log("Rewards JS Loaded");
    // Noooo yrrrr  hurt mne pn thay 6 mne pn nai saru lagtu butatleast vat toh kr yrrr khali sathe besye? haaaaaaa saruu but ek var sari rite bolav agar man hoy toh pehla jarak besu shanti thi? ok:((((((((((( tari sathe j chu baju maj chu coco nahhhhhhhhh nathi sathe evu feel thay 6 sathe j chu chal run karine error atav
    // ✅ Toastr Configuration
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "timeOut": "3000"
    };

    // ================================
    // 🔑 Save Rewards Form Validation & Submission
    // ================================

    $("#rewardForm").validate({
        rules: {
            RewardBatch: {
                required: true
            },
            RequiredAmount: {
                required: true,
                number: true,
                min: 1  // ✅ Ensures positive numbers only
            },
            RewardDescription: {
                required: true,
                minlength: 10
            }
        },
        messages: {
            RewardBatch: {
                required: "Please enter reward batch."
            },
            RequiredAmount: {
                required: "Please enter the required amount.",
                number: "Only numbers are allowed.",
                min: "Amount must be at least 1."
            },
            RewardDescription: {
                required: "Please enter the reward description.",
                minlength: "Description must be at least 10 characters."
            }
        },
        errorPlacement: function (error, element) {
            error.addClass("text-danger");
            error.insertAfter(element);
        },
        submitHandler: function (form, event) {
            event.preventDefault();

            const formData = new FormData(form);

            $.ajax({
                url: "/Rewards/SaveRewards",
                type: "POST",
                processData: false,
                contentType: false,
                data: formData,
                success: function (result) {
                    if (result.success) {
                        toastr.success(result.message);

                        // ✅ Redirect to the campaign list after success
                        setTimeout(() => {
                            window.location.href = '/Rewards/RewardsList';
                        }, 1500);
                    } else {
                        toastr.error(result.message);
                    }
                },
                error: function () {
                    toastr.error('An error occurred while saving the Reward!');
                }
            });
        }
    });
});

function showToast(message, type) {
    let bgColor = type === "success" ? "bg-success" : "bg-danger";
    let toastId = "toast-" + new Date().getTime();

    $("#toastContainer").append(
        <div id="${toastId}" class="toast align-items-center text-white ${bgColor} border-0 show" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    );

    let toastElement = new bootstrap.Toast(document.getElementById(toastId));
    toastElement.show();

    setTimeout(() => $("#" + toastId).fadeOut("slow", function () { $(this).remove(); }), 2000);
}


//$(document).ready(function () {
//    console.log("Rewards JS Loaded");

//    // ✅ Toastr Configuration
//    toastr.options = {
//        "closeButton": true,
//        "progressBar": true,
//        "timeOut": "3000"
//    };

//    // ================================
//    // 🔑 Save Rewards Form Validation & Submission
//    // ================================

//    $("#rewardForm").validate({
       
//        rules: {
//            RewardBatch: {
//                required: true
//            },
//            RequiredAmount: {
//                required: true,
//                number: true,
//                //min: 1
//            },
//            RewardDescription: {
//                required: true,
//                minlength: 10
//            }      
//        },
//        messages: {
//            RewardBatch: {
//                required: "Please enter reward batch."
//            },
//            RequiredAmount: {
//                required: "Please enter the required amount.",
//                number: "Please enter a valid number.",
//                //min: "Requirement amount must be at least 1."
//            },
//            RewardDescription: {
//                required: "Please enter the reward description.",
//                minlength: "Description must be at least 10 characters."
//            },
            
//        },
//        errorPlacement: function (error, element) {
//            error.addClass("text-danger");
//            error.insertAfter(element);
//        },


//        submitHandler: function (form, event) {
//            event.preventDefault();

//            const formData = new FormData(form);

//            $.ajax({
//                url: "/Rewards/SaveRewards",
//                type: "POST",
//                processData: false,
//                contentType: false,
//                data: formData,

//                success: function (result) {
//                    if (result.success) {
//                        toastr.success(result.message);

//                        // ✅ Redirect to the campaign list after success
//                        setTimeout(() => {
//                            window.location.href = '/Rewards/RewardsList';
//                        }, 1500);
//                    } else {
//                        toastr.error(result.message);
//                    }
//                },
//                error: function () {
//                    toastr.error('An error occurred while saving the Reward!.');
//                }
//            });
//        }
//    });
//});
