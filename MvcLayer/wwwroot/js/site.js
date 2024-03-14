// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
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
//Разбиение строки по разрядам числа
function digits_float(target) {    
    val = $(target).val().replace(/[^0-9,-]/g, '');
    val = val.replace(/\s/g, "");
    val = val.replace(/(?!^)-/g, '');    
    if (val.indexOf(",") != '-1') {
        first = val.substring(0, val.indexOf(",") + 1);  
        second = val.substring(val.indexOf(",") + 1, val.indexOf(",") + 3);    
        second = second.replace(/[^0-9]/g, '');        
        val = first + second;     
    }    
    val = val.replace(/\B(?=(\d{3})+(?!\d))/g, ' ');
    $(target).val(val);    
}

$(document).ready(function () {
    $('.js-chosen').chosen({
        no_results_text: 'Совпадений не найдено',
        placeholder_text_single: 'Выберите',
        width: '100%',
        disable_search_threshold: 5
    });
});

function setColor(element, color) {
    element.style.backgroundColor = color;
}

$('[type="date"]').attr('inputmode', 'none');
$('[type="date"]').on('keydown', (e) => {
    e.preventDefault();
    return false;
});

let arrayTextEnd = document.getElementsByClassName('text-end');

for (let value of arrayTextEnd) {    
    let val = Number(value.textContent.replace(/\s/g, '').replace(/,/g, '.'));        
    if (val != NaN && val < 0) {
        value.setAttribute('style', 'color:red');
    }      
}