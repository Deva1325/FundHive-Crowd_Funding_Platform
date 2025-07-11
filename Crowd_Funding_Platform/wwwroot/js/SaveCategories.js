﻿//$(document).ready(function () {

//    // ✅ Confirm delete function
//    window.confirmDelete = function (categoryId) {
//        if (confirm("Are you sure you want to delete this category?")) {
//            deleteCategory(categoryId);
//        }
//    }

//    // ✅ Delete AJAX function
//    function deleteCategory(categoryId) {
//        $.ajax({
//            url: `/Categories/Delete/${categoryId}`,   // Updated endpoint format
//            type: "DELETE",
//            success: function (response) {
//                if (response.success) {
//                    showToast(response.message, "success");
//                    setTimeout(() => location.reload(), 1000);
//                } else {
//                    showToast(response.message, "error");
//                }
//            },
//            error: function (xhr) {
//                console.error("Error:", xhr.responseText);
//                showToast("Failed to delete category!", "error");
//            }
//        });
//    }

//    function showToast(message, type) {
//        let bgColor = type === "success" ? "bg-success" : "bg-danger";
//        let toastId = "toast-" + new Date().getTime();

//        $("#toastContainer").append(`
//            <div id="${toastId}" class="toast align-items-center text-white ${bgColor} border-0 show" role="alert" aria-live="assertive" aria-atomic="true">
//                <div class="d-flex">
//                    <div class="toast-body">${message}</div>
//                    <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
//                </div>
//            </div>
//        `);

//        let toastElement = new bootstrap.Toast(document.getElementById(toastId));
//        toastElement.show();

//        setTimeout(() => $("#" + toastId).fadeOut("slow", function () { $(this).remove(); }), 3000);
//    }
//});



$(document).ready(function () {
    $("#categoryForm").on("submit", function (event) {
        event.preventDefault(); // Prevent default form submission

        if (!validateForm()) return; // Validate form before submission

        let formData = new FormData(this);

        $.ajax({
            url: "/Categories/SaveCategories",
            type: "POST",
            processData: false,
            contentType: false,
            data: formData,
            success: function (response) {
                if (response.success) {
                    showToast(response.message, "success");
                    setTimeout(() => {
                        window.location.href = "/Categories/CategoriesList";
                    }, 2000);
                } else {
                    showToast(response.message, "error");
                }
            },
            error: function () {
                showToast("An error occurred while saving the category!", "error");
            }
        });
    });

    function validateForm() {
        let isValid = true;
        $(".text-danger").text(""); // Clear previous errors

        let name = $("#Name").val().trim();
        let description = $("#Description").val().trim();

        if (name === "") {
            $("#Name").next(".text-danger").text("Name is required.");
            isValid = false;
        }

        if (description === "") {
            $("#Description").next(".text-danger").text("Description is required.");
            isValid = false;
        }

        return isValid;
    }

    function showToast(message, type) {
        let bgColor = type === "success" ? "bg-success" : "bg-danger";
        let toastId = "toast-" + new Date().getTime();

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

        setTimeout(() => $("#" + toastId).fadeOut("slow", function () { $(this).remove(); }), 2000);
    }
});
