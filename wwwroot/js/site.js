// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

const { Toast } = require("bootstrap");
const { signalR } = require("../lib/SignalR/signalr");

// Write your JavaScript code.
function Show() {
    var x = document.getElementById("password");
    if (x.type === "password") {
        x.type = "text";
        $('#invisible').css("display", "block");
        $('#visible').css("display", "none")

    } else {
        x.type = "password";
        $('#invisible').css("display", "none")
        $('#visible').css("display", "block")
    }
}

function ShowConfirm() {
    var x = document.getElementById("passwordConfirm");
    if (x.type === "password") {
        x.type = "text";
        $('#invisibleConfirm').css("display", "block");
        $('#visibleConfirm').css("display", "none")

    } else {
        x.type = "password";
        $('#invisibleConfirm').css("display", "none")
        $('#visibleConfirm').css("display", "block")
    }
}

$('#module-select').chosen(
    { max_selected_options: 4, width: "100%" }).addClass('form-control');

//photo upload function 
$(".custom-file-input").on("change", function () {
    var fileName = $(this).val().split("\\").pop();
    $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
});
//Table functions
function DeleteItem(btn) {
    $(btn).closest('tr').remove();
}

function AddItem(btn) {

    var table = document.getElementById('myTable');
    
    var row = table.getElementsByTagName('tr');

    var rowOuterHtml = row[row.length - 1].outerHTML;

    var lastrowIdx = document.getElementById('TlastIndex').value;

    var nextrowIdx = eval(lastrowIdx) + 1;

    document.getElementById('TlastIndex').value = nextrowIdx;

    rowOuterHtml = rowOuterHtml.replaceAll('_' + lastrowIdx + '_', '_' + nextrowIdx + '_');
    rowOuterHtml = rowOuterHtml.replaceAll('[' + lastrowIdx + ']', '[' + nextrowIdx + ']');
    rowOuterHtml = rowOuterHtml.replaceAll('-' + lastrowIdx, '-' + nextrowIdx);


    var newRow = table.insertRow();
    newRow.innerHTML = rowOuterHtml;

    var btnAddId = btn.id;
    var btnDeletedId = btnAddId.replaceAll('btn_add', 'btn_remove');

    var btnDelete = document.getElementById(btnDeletedId);

    btnDelete.classList.add('visible');
    btnDelete.classList.remove('invisible')

    var btnAdd = document.getElementById(btnAddId);
    btnAdd.classList.remove('visible');
    btnAdd.classList.add('invisible');

}

function onValueChanged() {
    var checkBox = document.getElementById('MyChk');
    var txt = document.getElementById('HourlyFeeId');
    if (checkBox.checked) {
        txt.removeAttribute('readonly');
    }
    else {
        txt.setAttribute('readonly', true);
        txt.value = "0";
    }
}

function Toasty() {
    var option = {
        animation: true,
        delay: 2000
    };
    var toast = document.getElementById("epicToast");
    var toastElement = new bootstrap.Toast(toast, option);
    toastElement.show();
}

function validate() {
    var list = document.getElementById("module-select");
    var form = document.getElementById("subForm");

    if (list.options.length == 0) {
        alert("Please select Modules")
    } else {
        form.submit();
    }
}

$(".custom-file-input").on("change", function () {
    var fileName = $(this).val().split("\\").pop();
    $(this).siblings(".custom-file-label").addClass("selected").html(fileName);
})


$('.fa-bell').click(function () {
    $('.count').hide();
})


function updateNotificationCount() {
    var count = 0;
    count = parseInt($('span.count').html()) || 0;

    count++;

    $('span.count').html(count);
}

//notification Hub

$(document).ready(function () {
    var connection = new signalR.HubConnectionBuilder().withUrl("/NotificationHub? userId=" + userId).build();

    connection.on('displayNotification', () => {
        updateNotificationCount();
    })

    $(function () {

        var placeHolderElement = $('#PlaceHolderHere');

        $('button[data-toggle="ajax-modal"]').click(function (event) {
            var url = $(this).data('url');

            $.get(url).done(function (data) {

                placeHolderElement.html(data);

                placeHolderElement.find('modal').modal.show();
            })

            placeHolderElement.on('click', '[data-save="modal"]', function (event) {
                var form = $(this).parents('.modal').find('form');

                var actionUrl = form.attr("action");

                var sendData = form.serialize();

                $.post(actionUrl, sendData).done(function (data) {
                    placeHolderElement.find(".modal").modal('hide');
                })
            })
        })
    })
})

   
jQueryAjaxPost = form => {
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                    location.reload();
                }
                else {
                    $('#form-modal .modal-title').html('Pay Fees');
                    $('#form-modal .modal-body').html(res.html);
                    $("#form-modal").modal('show');
                }

            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

$(document).ready(function () {
    $("#tableid").freezeHeader({ 'height': '300px' });
})

