$(document).ready(function () {
    console.log("Under Campaign Creator");

    // ✅ Toastr Configuration
    toastr.options = {
        "closeButton": true,
        "progressBar": true,
        "timeOut": "3000"
    };

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
                    if (result.success) {
                        toastr.success(result.message);
                        console.log("Response:", result);

                        // ✅ Redirect to the campaign list after success
                        setTimeout(() => {

                            window.location.href = '/Campaign/ApplyForCreator';
                        }, 1500);
                    } else {
                        toastr.error(result.message);
                    }
                },              
                error: function () {
                    alert("An error occurred while submitting the application.");
                }
            });

            return false;
        }
    });
});