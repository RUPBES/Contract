﻿// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function ShowContent(event) {
    event.classList.toggle("show");
}

function ShowModal(Action, Controller, id) {
    $.ajax({
        type: 'POST',
        /*url: `@Url.Action(${Action},${Controller})`,*/
        url: "/Amendments/Create",
        dataType: 'html',
     /*   data: { id: id },*/
        success: function (data) {
            $('#modal-container').html(data);
        },
        error: function (ex) {
            alert('Упс');
        }
    })
}

window.onclick = function (event) {
    if (!event.target.matches('.dropbtn')) {
        var dropdowns = document.getElementsByClassName("dropdown-content");
        var i;
        for (i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('show')) {
                openDropdown.classList.remove('show');
            }
        }
    }
}