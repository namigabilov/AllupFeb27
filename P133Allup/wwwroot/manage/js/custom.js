$(document).ready(function () {

    let isMain = $(this).is(':checked');

    if (isMain) {
        $('#selectInput').addClass('d-none');
        $('#fileInput').removeClass('d-none');
    }
    else {
        $('#selectInput').removeClass('d-none');
        $('#fileInput').addClass('d-none');
    }

    $('#IsMain').click(function (e) {

        let isMain = $(this).is(':checked');

        if (isMain) {
            $('#selectInput').addClass('d-none');
            $('#fileInput').removeClass('d-none');
        }
        else {
            $('#selectInput').removeClass('d-none');
            $('#fileInput').addClass('d-none');
        }
    })
})