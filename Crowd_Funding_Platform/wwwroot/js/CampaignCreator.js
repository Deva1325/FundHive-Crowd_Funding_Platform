//$(document).ready(function () {
//    console.log("Under Campaign Creator");

//    //// ✅ Toastr Configuration
//    //toastr.options = {
//    //    "closeButton": true,
//    //    "progressBar": true,
//    //    "timeOut": "3000"
//    //};
//    // ================================
//    // 🔥 Toastr Configuration
//    // ================================
//    toastr.options = {
//        "closeButton": true,
//        "debug": false,
//        "newestOnTop": true,
//        "progressBar": true,
//        "positionClass": "toast-top-right",
//        "preventDuplicates": false,
//        "showDuration": "300",
//        "hideDuration": "1000",
//        "timeOut": "5000",
//        "extendedTimeOut": "1000",
//        "showEasing": "swing",
//        "hideEasing": "linear",
//        "showMethod": "fadeIn",
//        "hideMethod": "fadeOut"
//    };


//    $("#applyForCreatorForm").validate({
//        rules: {
//            ImageFile: {
//                required: true,
//                //extension: "pdf|jpg|jpeg|png"
//            },
//            DocumentType: {
//                required: true
//            }
//        },
//        messages: {
//            ImageFile: {
//                required: "Please upload a document.",
//                //extension: "Invalid file format. Only PDF, JPG, JPEG, and PNG files are allowed."
//            },
//            DocumentType: {
//                required: "Please select a document type."
//            }
//        },
//        submitHandler: function (form, event) {
//            event.preventDefault(); // ✅ Prevent default form submission

//            var formData = new FormData(form);

//            $.ajax({
//                url: '/Campaign/ApplyForCreator',
//                type: 'POST',
//                data: formData,
//                /*processData: false,
//                contentType: false,*/

//                success: function (result) {
//                    if (result.success) {
//                        //console.log("Response:", result);
//                        toastr.success("Application submited succesfully");
//                        // ✅ Redirect to the campaign list after success
//                        setTimeout(() => {
//                        }, 1500);
//                    } else {
//                        toastr.error(result.message);
//                    }
//                },
//                error: function () {
//                    toastr.error(result.message || "An error occurred while submitting the application.");
//                }
//            });

//            return false;
//        }
//    });
//});


$(document).ready(function () {

    var userId = '@ViewBag.CurrentUserId'; // or from session

    // ✅ Show success message if stored after redirect
    let creatorAppSuccessMsg = sessionStorage.getItem("creatorAppSuccess");
    if (creatorAppSuccessMsg) {
        toastr.success(creatorAppSuccessMsg);
        sessionStorage.removeItem("creatorAppSuccess"); // Clear it after showing
    }


    // ✅ Fetch current creator application status
    $.ajax({
        url: '/Campaign/GetCreatorApplicationStatus',
        type: 'GET',
        data: { userId: userId },
        success: function (response) {
            if (response.status === "Pending") {
                $("#creatorForm").hide();
                $("#statusMessage").text("Your application is pending. Please wait for admin response.").show();
            } else if (response.status === "Approved") {
                $("#creatorForm").hide();
                $("#statusMessage").text("You are already approved as a creator.").show();
            } else {
                $("#creatorForm").show();
                $("#statusMessage").hide();
            }
        },
        error: function () {
            $("#creatorForm").show();
            $("#statusMessage").hide();
        }
    });

    // ✅ jQuery Validation
    $("#applyForCreatorForm").validate({
        rules: {
            ImageFile: {
                required: true
            },
            DocumentType: {
                required: true
            }
        },
        messages: {
            ImageFile: {
                required: "Please upload a document."
            },
            DocumentType: {
                required: "Please select a document type."
            }
        }
    });

    // ✅ AJAX Submit Handler
    $("#btnSubmit").on('click', function (e) {
        e.preventDefault(); // 🚫 Prevent form from submitting normally

        var isValid = $("#applyForCreatorForm").valid();
        if (!isValid) {
            return;
        }

        var formData = new FormData(document.getElementById("applyForCreatorForm"));

        $.ajax({
            url: '/Campaign/ApplyForCreator',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            success: function (result) {
                if (result && result.success !== undefined) {
                    if (result.success) {
                        // ✅ Store success message temporarily
                        sessionStorage.setItem("creatorAppSuccess", result.message || "Application submitted successfully!");

                        setTimeout(() => {
                            location.reload(); // ✅ Reload page
                        }, 1000);
                    } else {
                        toastr.error(result.message || "Failed to submit application.");
                    }
                }

            },
            error: function (xhr) {
                let errorMsg = "An error occurred while submitting the application.";
               
                toastr.error(errorMsg);
            }
        });
    });

});







//$(document).ready(function () {
//    console.log("Under Campaign Creator");

//    // ================================
//    // 🔥 Toastr Configuration
//    // ================================
//    toastr.options = {
//        "closeButton": true,
//        "debug": false,
//        "newestOnTop": true,
//        "progressBar": true,
//        "positionClass": "toast-top-right",
//        "preventDuplicates": false,
//        "showDuration": "300",
//        "hideDuration": "1000",
//        "timeOut": "5000",
//        "extendedTimeOut": "1000",
//        "showEasing": "swing",
//        "hideEasing": "linear",
//        "showMethod": "fadeIn",
//        "hideMethod": "fadeOut"
//    };

//   // toastr.success("Toastr is working!");


//    //// ✅ Toastr Configuration
//    //toastr.options = {
//    //    "closeButton": true,
//    //    "progressBar": true,
//    //    "timeOut": "3000"
//    //};

//    var userId = '@ViewBag.CurrentUserId'; // or from session


//    $.ajax({
//        url: '/Campaign/GetCreatorApplicationStatus',
//        type: 'GET',
//        data: { userId: userId },
//        success: function (response) {
//            if (response.status === "Pending") {
//                $("#creatorForm").hide();
//                $("#statusMessage").text("Your application is pending. Please wait for admin response.").show();
//            } else if (response.status === "Approved") {
//                $("#creatorForm").hide();
//                $("#statusMessage").text("You are already approved as a creator.").show();
//            } else {
//                $("#creatorForm").show();
//                $("#statusMessage").hide();
//            }
//        },
//        error: function () {
//            $("#creatorForm").show();
//            $("#statusMessage").hide();
//        }
//    });
//    $("#btnSubmit").on('click', function () {
//        var valid = $("#applyForCreatorForm").valid();


//        if (valid) {
//            var formData = new FormData(document.getElementById("applyForCreatorForm"));

//            $.ajax({
//                url: '/Campaign/ApplyForCreator',
//                type: 'POST',
//                data: formData,
//                processData: false,
//                contentType: false,

//                success: function (result) {
//                    console.log(result);
//                    // Log the response
//                    if (result && result.success !== undefined) {
//                        if (result.success) {
//                            toastr.success("Application submited succesfully");  
//                            //alert("Application submited succesfully");

//                            setTimeout(() => {
//                                //window.location.href = '/Campaign/ApplyForCreator';
//                            }, 2000);
//                        } else {
//                            toastr.error(result.message); 
//                        }
//                    }
//                },
//                error: function () {
//                    toastr.error(result.message || "An error occurred while submitting the application.");
//                    //alert("An error occurred while submitting the application.");
//                }
//            });
//        }
//    })

//    $("#applyForCreatorForm").validate({
//        rules: {
//            ImageFile: {
//                required: true
//            },
//            DocumentType: {
//                required: true
//            }
//        },
//        messages: {
//            ImageFile: {
//                required: "Please upload a document."
//               /* extension: "Invalid file format. Only PDF, JPG, JPEG, and PNG files are allowed."*/
//            },
//            DocumentType: {
//                required: "Please select a document type."
//            }
//        },
//        //submitHandler: function (form, event) {
//        //    event.preventDefault(); // ✅ Prevent default form submission

           

//        //    return false;
//        //}
//    });
//});