﻿function checkTwoSelectValues(firstId, secondId, areaId, buttonId, valueStyle) {
    $(firstId).change(function () {
        let firstValue = $(firstId).find(":selected").val();
        let secondValue = $(secondId).find(":selected").val();
        
        if (firstValue === secondValue) {
            let button = $(buttonId);
            button.attr("type", "button");
            $(areaId).attr("style", valueStyle);          
        }
        else {
            let button = $(buttonId);
            button.attr("type", "submit");
            $(areaId).removeAttr("style", valueStyle);            
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
    /*datepicker("input.datepickers");*/
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

function fillPhoneInput() {
    document.querySelector(".phone-number-pattern-fill").addEventListener("keydown", function (e) {
        const txt = this.value;
        // больше чем 14 символов нельзя ввести, игнорируем ввод, разрешаем удалить символ
        if ((txt.length == 14 || e.which == 32) & e.which !== 8) e.preventDefault();

        if (txt.length == 4 && e.which !== 8) {
            this.value = this.value + " ";
        }
        else if ((txt.length == 8 || txt.length == 11) && e.which !== 8) {
            this.value = this.value + "-";
        }

    });
}

function confirmDelete() {
    if (confirm('Вы уверены, что хотите удалить?')) {
        return true;
    }
    else {
        return false;
    }
}