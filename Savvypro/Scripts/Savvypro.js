$('#fileSelectBtn').on('change', function () {
    var file = this.files[0];
    var fileSizeMB = (file.size / (1024 * 1024)).toFixed(2);
    $('.custom-file-label').text(file.name);
    if (fileSizeMB > 5) {
        toastr.error("File Size should less then 5 MB.");
    } else {
        $('#fileUploadBtn').attr('disabled', false);
    }
})

function handleFileUpload() {

    event.preventDefault();

    var image = document.getElementById('fileSelectBtn').files[0];

    if (!image.type.match('image.*')) {
        //continue;
    }

    var reader = new FileReader();

    reader.onload = function (e) {
        $('#imagePreview').attr('src', e.target.result);

        //upload to server
        var file = document.getElementById('fileSelectBtn').files[0];
        var formData = new FormData();
        formData.set("file", file, file.name)
        console.log(formData);

        $.ajax({
            type: 'POST',
            url: '/home/SaveImage',
            data: formData,
            cache: false,
            contentType: false,
            processData: false,
            async: true,
            beforeSend: function () {
                $(".progress").css('visibility','visible');
            },
            success: function (data) {
                console.log("success");
                console.log(data);
                toastr.success(data["responseText"]);
                $("#ocrResult").parent().addClass("border border-dark")
                $("#ocrResult").html(data["ocrResult"].replace(/(?:\r\n|\r|\n)/g, '<br>'));

            },
            error: function (data) {
                console.log("error");
                console.log(data);
                toastr.error(data.responseJSON["responseText"]);
            },
            complete: function () {
                $(".progress").css('visibility', 'hidden');
                $('#fileUploadBtn').attr('disabled', true);
            }
        });
    }

    // Read in the image file as a data URL.
    reader.readAsDataURL(image);
}


