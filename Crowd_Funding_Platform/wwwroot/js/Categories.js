document.addEventListener("DOMContentLoaded", function () {
    attachDeleteEvent();
    attachFormSubmitHandler();
});

// Function to handle Delete confirmation
function confirmDelete(categoryId) {
    if (confirm("Are you sure you want to delete this category?")) {
        fetch(`/Categories/DeleteCategory/${categoryId}`, {
            method: "DELETE"
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    showToast("successToast", "Category deleted successfully.");
                    setTimeout(() => location.reload(), 1500);
                } else {
                    showToast("errorToast", "Failed to delete category.");
                }
            })
            .catch(error => {
                showToast("errorToast", "An error occurred while deleting.");
            });
    }
}

// Function to show toast notifications
function showToast(toastId, message) {
    document.getElementById(toastId).querySelector(".toast-body").innerText = message;
    let toastElement = new bootstrap.Toast(document.getElementById(toastId));
    toastElement.show();
}

// Attach event listeners for Delete buttons
function attachDeleteEvent() {
    document.querySelectorAll(".btn-delete-category").forEach(button => {
        button.addEventListener("click", function () {
            let categoryId = this.getAttribute("data-id");
            confirmDelete(categoryId);
        });
    });
}

// Attach validation and form submission handling
function attachFormSubmitHandler() {
    const form = document.getElementById("categoryForm");
    if (form) {
        form.addEventListener("submit", function (event) {
            event.preventDefault();
            if (validateForm()) {
                form.submit();
            }
        });
    }
}

// Function to validate form
function validateForm() {
    let name = document.getElementById("CategoryName").value.trim();
    let description = document.getElementById("CategoryDescription").value.trim();

    if (name === "") {
        showToast("errorToast", "Category name is required.");
        return false;
    }
    if (description === "") {
        showToast("errorToast", "Category description is required.");
        return false;
    }
    return true;
}
