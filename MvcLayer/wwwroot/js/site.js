// получаем координаты элемента в контексте документа
function getCoords(elem) {
    let box = elem.getBoundingClientRect();

    return {
        top: box.top + window.pageYOffset,
        right: box.right + window.pageXOffset,
        bottom: box.bottom + window.pageYOffset,
        left: box.left + window.pageXOffset
    };
}
function showResultMessage(objSelector, htmlMessage) {
    $(objSelector).html('');
    $(objSelector).html(htmlMessage);
    $(objSelector).show(200);

    setTimeout(() => {
        $(objSelector).hide(400);
        //$(objSelector).html('');
    }, 2000);

}

function changeClassByClickButton(buttonClickObj, obj, removeClass, addClass) {
    $(buttonClickObj).click(() => {
        let elem = document.querySelector(obj);
        if (elem) {
            elem.classList.remove(removeClass);
            elem.classList.add(addClass);
        }

    });
}

function GoNextStep() {
    let points = document.querySelectorAll("div.line-item");
    if (points) {
        points.forEach((e) => {
            if (!e.classList.contains("done") && !e.classList.contains("unactive-step")) {
                e.classList.add("done");
            }
        });
    }
    document.querySelector(".line-item.unactive-step").classList.remove("unactive-step");
}

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