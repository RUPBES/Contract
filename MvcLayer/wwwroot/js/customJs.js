/** Проверка на выбор select с одним и тем же значением. Если хотябы один совпадает, все select помечаются в красный цвет, а кнопка отправки/сохранения блокируется!
 * /
 * @param {any} firstId
 * @param {any} secondId
 * @param {any} areaId
 * @param {any} buttonId
 * @param {any} valueStyle
 */
function checkTwoSelectValues(firstId, secondId, areaId, buttonId, valueStyle) {
    $(firstId).change(function () {
        let firstValue = $(firstId).find(":selected").val();
        let secondValue = $(secondId).find(":selected").val();
        const fieldset = $(areaId);//.children(":first");

        if (firstValue === secondValue) {
            let button = $(buttonId);
            button.attr("type", "button");
            fieldset.attr("style", valueStyle);
        }
        else {
            let button = $(buttonId);
            button.attr("type", "submit");
            fieldset.removeAttr("style", valueStyle);
        }

    });
}
function checkThreeSelectValuesWithChosenStyle(firstId, secondId, thirdId, buttonId, styleName = 'same_value_chosen') {

    $(firstId).change(function () {
        changeStatusSelectValue(firstId, secondId, thirdId, buttonId, styleName);
    });
    $(secondId).change(function () {
        changeStatusSelectValue(firstId, secondId, thirdId, buttonId, styleName);
    });
    $(thirdId).change(function () {
        changeStatusSelectValue(firstId, secondId, thirdId, buttonId, styleName);
    });
}
function changeStatusSelectValue(selectId, secondId, thirdId, buttonId, styleName) {
    const button = $(buttonId);
    const classSLCT = '.organization-fieldset-slct';
    //for js-chosen
    //const selectObjOne = $(`${selectId}_chosen`);
    //const selectObjTwo = $(`${secondId}_chosen`);
    //const selectObjThree = $(`${thirdId}_chosen`);


    const selectObjOne = $(`${selectId}`);
    const selectObjTwo = $(`${secondId}`);
    const selectObjThree = $(`${thirdId}`);


    let valueObjOne = +($(selectId).find(":selected").val());
    let valueObjTwo = +($(secondId).find(":selected").val());
    let valueObjThree = +($(thirdId).find(":selected").val());

    if ((valueObjOne === valueObjTwo) && (valueObjOne !== 0 && valueObjTwo !== 0)) {
        selectObjOne.closest(classSLCT).addClass(styleName);
        //selectObjTwo.closest("fieldset").addClass(styleName);
        $(buttonId).attr("type", "button");
    }
    if ((valueObjOne === valueObjThree) && ((valueObjOne) !== 0 && (valueObjThree) !== 0)) {
        selectObjOne.closest(classSLCT).addClass(styleName);
        //selectObjThree.closest("fieldset").addClass(styleName);
        $(buttonId).attr("type", "button");
    }
    if ((valueObjTwo === valueObjThree) && ((valueObjTwo) !== 0 && (valueObjThree) !== 0)) {

        selectObjTwo.closest(classSLCT).addClass(styleName);
        //selectObjThree.closest("fieldset").addClass('same_value_chosen');
        $(buttonId).attr("type", "button");
    }

    //if ((valueObjOne === valueObjTwo) && (typeof (valueObjOne) === "number" && typeof (valueObjTwo) === "number")) {
    //    selectObjOne.closest(classSLCT).addClass(styleName);
    //    //selectObjTwo.closest("fieldset").addClass(styleName);
    //    $(buttonId).attr("type", "button");
    //}
    //if ((valueObjOne === valueObjThree) && (typeof (valueObjOne) === "number" && typeof (valueObjThree) === "number")) {
    //    selectObjOne.closest(classSLCT).addClass(styleName);
    //    //selectObjThree.closest("fieldset").addClass(styleName);
    //    $(buttonId).attr("type", "button");
    //}
    //if ((valueObjTwo === valueObjThree) && (typeof (valueObjTwo) === "number" && typeof (valueObjThree) === "number")) {

    //    selectObjTwo.closest(classSLCT).addClass(styleName);
    //    //selectObjThree.closest("fieldset").addClass('same_value_chosen');
    //    $(buttonId).attr("type", "button");
    //}
    const firstNotSame = valueObjOne !== valueObjTwo && valueObjOne !== valueObjThree;
    const secondNotSame = valueObjTwo !== valueObjOne && valueObjTwo !== valueObjThree;
    /*    const thirdNotSame = valueObjThree !== valueObjTwo && valueObjThree !== valueObjOne;*/

    if (firstNotSame && secondNotSame) {
        selectObjTwo.closest(classSLCT).removeClass(styleName);
    }
    //if (thirdNotSame) {
    //    selectObjThree.closest("fieldset").removeClass(styleName);
    //}

    if (document.querySelectorAll(`.${styleName}`).length < 1) {
        //document.querySelectorAll(`.${styleName}`).forEach((elem) => {
        //    elem.closest("fieldset").classList.remove(styleName);
        //});
        button.attr("type", "submit");
    }
}
function changeButtonType(buttonId, NumDCId) {
    let button = $(buttonId);
    let input = $(NumDCId);
    button.attr("type", "submit");
    input.attr("disabled", true);
}

/** */
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


function handleError(error/*, urlRequest, progressObjID = null*/) {
    var errorMes = error.responseText;
    alert(errorMes);
}




// ----- Сообщение о подтверждении действия, либо да либо нет!
/*
В ссылку вставить class и data-message атрибуты!
 <a asp-action="******" ..........
            class="modal-link"
            data-message="........ будут удалены. Продолжить?"></a>

*/
function contractAlert() {
    //const modal = document.getElementById('alert-modal');
    //const modalTitle = document.getElementById('alert-modal_title');
    //const modalMessage = document.getElementById('alert-modal_message');
    //const modalConfirm = document.getElementById('alert-modal_confirm');
    //const modalCancel = document.getElementById('alert-modal_cancel');

    let currentUrl = '';

    document.querySelectorAll('.modal-link').forEach(link => {
        link.addEventListener('click', function (e) {
            e.preventDefault();

            currentUrl = this.href;
            document.getElementById('alert-modal_title').textContent = this.getAttribute('data-title') || 'Предупреждение';
            document.getElementById('alert-modal_message').textContent = this.getAttribute('data-message') || 'Вы уверены?';

            document.getElementById('alert-modal').style.display = 'block';
        });
    });

    document.getElementById('alert-modal_confirm').addEventListener('click', function () {
        window.location.href = currentUrl;
    });

    document.getElementById('alert-modal_cancel').addEventListener('click', function () {
        document.getElementById('alert-modal').style.display = 'none';
    });
};

contractAlert();