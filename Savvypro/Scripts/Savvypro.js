$('#fileSelectBtn').on('change', function () {
    var image = this.files[0];
    var imageSizeMB = (image.size / (1024 * 1024)).toFixed(2);
    $('.custom-file-label').text(image.name);

    if (!image.type.match('image.*')) {
        toastr.error("Only accept image files.");
        return;
    }

    if (imageSizeMB > 3) {
        toastr.error("File Size should less than 3 MB.");
        return;
    } else {
        $('#fileUploadBtn').attr('disabled', false);
        //preview picture
        var reader = new FileReader();
        // Read in the image file as a data URL.
        reader.readAsDataURL(image);
        reader.onload = function (e) {
            $('#imagePreview').attr('src', e.target.result);
        }
    }
})

function handleFileUpload() {

    event.preventDefault();

    //upload to server
    var file = document.getElementById('fileSelectBtn').files[0];
    var formData = new FormData();
    formData.set("file", file, file.name)

    $.ajax({
        type: 'POST',
        url: '/home/SaveImage',
        data: formData,
        cache: false,
        contentType: false,
        processData: false,
        async: true,
        beforeSend: function () {
            $(".progress-bar").css('width', 0);
            $(".progress").css('visibility', 'visible');
        },
        xhr: function () {
            /*progress bar*/
            var xhr = new window.XMLHttpRequest();
            xhr.upload.addEventListener("progress", function (evt) {
                if (evt.lengthComputable) {
                    var percentComplete = evt.loaded / evt.total;
                    $(".progress-bar").css('width', percentComplete * 100 + '%');
                }
            }, false);
            xhr.upload.onloadend = function () {
                toastr.success("Successfully uploaded image file.");
                $(".loader").show();
            }
            return xhr;
        },
        success: function (data) {
            console.log("success");
            console.log(data);
            toastr.success(data["responseText"]);
            $(".loader").hide();
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
            $(".loader").hide();
        }
    });
}


