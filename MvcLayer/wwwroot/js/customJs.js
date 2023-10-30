function checkTwoSelectValues(firstId, secondId, buttonId, valueStyle) {
    $(firstId).change(function () {
        let firstValue = $(firstId).find(":selected").val();
        let secondValue = $(secondId).find(":selected").val();

        if (firstValue === secondValue) {
            let button = $(buttonId);
            button.attr("type", "button");
            $(firstId).attr("style", valueStyle);
            $(secondId).attr("style", valueStyle);
        }
        else {
            let button = $(buttonId);
            button.attr("type", "submit");
            $(firstId).removeAttr("style", valueStyle);
            $(secondId).removeAttr("style", valueStyle);
        }

    });
}

function changeButtonType(buttonId, NumDCId) { 
    let button = $(buttonId);
    let input = $(NumDCId);
    button.attr("type", "submit");
    input.attr("disabled", true);
}

$(document).ready(function () {

    datepickerNull("input.datepickersNull");
    dateTimepicker("input.datetimepickers");
    datepicker("input.datepickers");
});

function paginationFunc(page_number, total_page) {
    
    let prev_pag_btn = document.querySelector(".previous_btn");
    let next_pag_btn = document.querySelector(".next_btn");
    let active_btn = document.querySelectorAll("a.page-link");

    document.querySelectorAll('.pag_btn_num').forEach((e) => {
        e.classList.remove("active");
    });
    active_btn.forEach((el) => {

        if (+el.getAttribute('value') === page_number) {
            el.parentElement.classList.add("active");
        }
    });

    if (page_number <= 1) {
        prev_pag_btn.classList.add("disabled");
    }
    else {
        prev_pag_btn.classList.remove("disabled");
    }

    if (page_number >= total_page) {
        next_pag_btn.classList.add("disabled");
    }
    else {
        next_pag_btn.classList.remove("disabled");
    }
}

function addition_nav_bar() {
    let btn_left_bar = document.querySelector('.link_add_emp');
    let new_emp = document.querySelector('.new_emp');
    let btn_panel_search = document.querySelector('.btn_panel_search');

    btn_left_bar.addEventListener('click', () => {
        btn_panel_search.classList.toggle('panel_active');
        btn_left_bar.classList.toggle('panel_active');
        new_emp.classList.toggle('btn_new_emp_active');
    });
}