//$(document).ready(function () {
//    console.log("Under Ready");

//    $("#applyForCreatorForm").validate({

//        rules: {
//            ImageFile: {
//                required: true,
//                extension: "pdf|jpg|jpeg|png"
//            },
//            DocumentType: {
//                required: true
//            }
//        },
//        messages: {
//            ImageFile: {
//                required: "Please upload a document.",
//                extension: "Invalid file format. Only PDF, JPG, JPEG, and PNG files are allowed."
//            },
//            DocumentType: {
//                required: "Please select a document type."
//            }
//        },
//        submitHandler: function (form) {
//            var formData = new FormData(form);

//            console.log("submithandler");
//            $.ajax({
//                url: '/Campaign/ApplyForCreator',
//                type: 'POST',
//                data: formData,
//                processData: false,
//                contentType: false,
//                success: function (response) {
//                    if (response.success) {
//                        showToast(response.message, 'bg-success');
//                    } else {
//                        showToast(response.message, 'bg-danger');
//                    }
//                },
//                error: function () {
//                    showToast("An error occurred while submitting the application.", 'bg-danger');
//                }
//            });

//            return false;
//        }

//    });

//    //// Toast display function
//    //function showToast(message, bgColor) {

//    //    console.log("showToast");


//    //    $('#toastMessage').text(message);
//    //    $('#responseToast')
//    //        .removeClass('bg-primary bg-success bg-danger')
//    //        .addClass(bgColor);
//    //    var toastElement = new bootstrap.Toast(document.getElementById('responseToast'));
//    //    toastElement.show();
//    //}
//});


$(document).ready(function () {
    console.log("Under Ready");

    $("#applyForCreatorForm").validate({
        rules: {
            ImageFile: {
                required: true,
                extension: "pdf|jpg|jpeg|png"
            },
            DocumentType: {
                required: true
            }
        },
        messages: {
            ImageFile: {
                required: "Please upload a document.",
                extension: "Invalid file format. Only PDF, JPG, JPEG, and PNG files are allowed."
            },
            DocumentType: {
                required: "Please select a document type."
            }
        },
        submitHandler: function (form, event) {
            event.preventDefault(); // ✅ Prevent default form submission

            var formData = new FormData(form);

            $.ajax({
                url: '/Campaign/ApplyForCreator',
                type: 'POST',
                data: formData,
                /*processData: false,
                contentType: false,*/

                success: function (result) {
                    alert(result.message);
                    if (result.success) {

                        alert(' document sent Successfully');
                        window.location.href = '/Account/Login';
                    }
                    else {
                        alert('document does not submitted.');
                    }
                },
                //beforeSend: function () {
                //    $(".btn-primary").prop("disabled", true).text("Uploading...");
                //},
                //success: function (response) {
                //    $(".btn-primary").prop("disabled", false).text("Create");

                //    if (response.success) {
                //        showToast(response.message, 'bg-success');
                //        setTimeout(() => window.location.reload(), 2000);
                //    } else {
                //        showToast(response.message, 'bg-danger');
                //    }
                //},
                
                error: function () {
                    alert("An error occurred while submitting the application.");
                }
            });

            return false;
        }
    });

    
});
