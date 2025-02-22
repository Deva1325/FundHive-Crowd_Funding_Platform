    $(document).ready(function () {
        $("#campaignForm").validate({
            rules: {
                Title: {
                    required: true,
                    minlength: 5,
                    maxlength: 100
                },
                Description: {
                    required: true,
                    minlength: 10
                },
                Requirement: {
                    required: true,
                    number: true,
                    min: 1
                },
                RaisedAmount: {
                    required: true,
                    number: true,
                    min: 1
                },
                StartDate: {
                    required: true,
                    date: true
                },
                EndDate: {
                    required: true,
                    date: true
                },
                CategoryId: {
                    required: true
                }
            },
            messages: {
                Title: {
                    required: "Please enter a campaign title.",
                    minlength: "Campaign title must be at least 5 characters.",
                    maxlength: "Campaign title cannot exceed 100 characters."
                },
                Description: {
                    required: "Please enter a campaign description.",
                    minlength: "Description must be at least 10 characters."
                },
                Requirement: {
                    required: "Please enter the required amount.",
                    number: "Please enter a valid number.",
                    min: "Requirement amount must be at least 1."
                },
                RaisedAmount: {
                    required: "Please enter the contribution amount.",
                    number: "Please enter a valid number.",
                    min: "Contribution amount must be at least 1."
                },
                StartDate: {
                    required: "Please enter the start date.",
                    date: "Please enter a valid date."
                },
                EndDate: {
                    required: "Please enter the end date.",
                    date: "Please enter a valid date."
                },
                CategoryId: {
                    required: "Please select a category."
                }
            },
            errorPlacement: function (error, element) {
                error.addClass("text-danger"); // Apply Bootstrap text-danger for styling
                error.insertAfter(element);
            },
            submitHandler: function (form, event) {
                event.preventDefault();
                const formData = new FormData(form);

                $.ajax({
                    url: '/ManageCampaigns/SaveCampaigns',
                    type: 'POST',
                    processData: false,
                    contentType: false,
                    data: formData,
                    success: function (result) {
                        alert(result.message);
                        if (result.success) {
                            alert('Successfully saved the campaign.');
                            location.reload();
                        }
                    },
                    error: function () {
                        alert('An error occurred while saving the campaign.');
                    }
                });
            }
        });
    });
