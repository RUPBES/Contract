
////    проверка вводимых дат ////
//dateContract(a) - Дата контракта
//beginWork(b) - Дата начало работы
//endWork(c) - Дата окончания работы
//enteringTm(d) - Дата ввода объекта
//contractTm(e) - Дата дествия договора
//a <= b < c <= d < e
function initializeDatepickers() {
    // Initialize date fields
    const dateFields = {
        DateDC: "#DateDC",
        DateBeginWork: "#DateBeginWork",
        DateEndWork: "#DateEndWork",
        EnteringTerm: "#EnteringTerm",
        ContractTerm: "#ContractTerm"
    };

    // Common datepicker options
    const datepickerOptions = {
        format: "d MM yyyy",
        language: "ru",
        calendarWeeks: true,
        autoclose: true,
        keyboardNavigation: false,
        assumeNearbyYear: true,
        toggleActive: true,
    };

    // Initialize datepickers
    Object.entries(dateFields).forEach(([field, selector]) => {
        $(selector).datepicker(datepickerOptions)
            .on('changeDate', () => updateDates(field));
    });

    function getDateValue(selector) {
        return $(selector).datepicker('getDate');
    }

    function updateDatepicker(selector, date) {
        $(selector).datepicker('update', date);
    }

    function setStartDate(selector, date) {
        $(selector).datepicker('setStartDate', date);
    }

    function addDays(date, days) {
        const result = new Date(date);
        result.setDate(result.getDate() + days);

        const day = String(result.getDate()).padStart(2, '0');
        const month = String(result.getMonth() + 1).padStart(2, '0');
        const year = result.getFullYear();

        return `${day}-${month}-${year}`;
    }

    function updateDates(changedField) {
        const dates = {
            DateDC: getDateValue(dateFields.DateDC),
            DateBeginWork: getDateValue(dateFields.DateBeginWork),
            DateEndWork: getDateValue(dateFields.DateEndWork),
            EnteringTerm: getDateValue(dateFields.EnteringTerm),
            ContractTerm: getDateValue(dateFields.ContractTerm)
        };

        const changed = dates[changedField];
        if (!changed) return;

        const changedStr = changed.toDateString('d-MM-yyyy');
        const nextDay = addDays(new Date(changedStr), 1);
        const nextTwoDay = addDays(new Date(changedStr), 2);
        const prevDay = addDays(new Date(changedStr), -1);
        const prevTwoDay = addDays(new Date(changedStr), -2);

        switch (changedField) {
            case 'DateDC':
                if (!dates.DateBeginWork || changed > dates.DateBeginWork) updateDatepicker(dateFields.DateBeginWork, changed);
                if (!dates.DateEndWork || changed >= dates.DateEndWork) updateDatepicker(dateFields.DateEndWork, nextDay);
                if (!dates.EnteringTerm || changed >= dates.EnteringTerm) updateDatepicker(dateFields.EnteringTerm, nextDay);
                if (!dates.ContractTerm || changed >= dates.ContractTerm) updateDatepicker(dateFields.ContractTerm, nextTwoDay);

                setStartDate(dateFields.DateBeginWork, changed);
                setStartDate(dateFields.DateEndWork, nextDay);
                setStartDate(dateFields.EnteringTerm, nextDay);
                setStartDate(dateFields.ContractTerm, nextTwoDay);
                break;

            case 'DateBeginWork':
                if (!dates.DateDC || changed < dates.DateDC) updateDatepicker(dateFields.DateDC, changed);
                if (!dates.DateEndWork || changed >= dates.DateEndWork) updateDatepicker(dateFields.DateEndWork, nextDay);
                if (!dates.EnteringTerm || changed >= dates.EnteringTerm) updateDatepicker(dateFields.EnteringTerm, nextDay);
                if (!dates.ContractTerm || changed >= dates.ContractTerm) updateDatepicker(dateFields.ContractTerm, nextTwoDay);

                setStartDate(dateFields.DateEndWork, nextDay);
                setStartDate(dateFields.EnteringTerm, nextDay);
                setStartDate(dateFields.ContractTerm, nextTwoDay);
                break;

            case 'DateEndWork':
                if (!dates.DateDC || changed <= dates.DateDC) updateDatepicker(dateFields.DateDC, prevDay);
                if (!dates.DateBeginWork || changed <= dates.DateBeginWork) updateDatepicker(dateFields.DateBeginWork, prevDay);
                if (!dates.EnteringTerm || changed > dates.EnteringTerm) updateDatepicker(dateFields.EnteringTerm, changed);
                if (!dates.ContractTerm || changed >= dates.ContractTerm) updateDatepicker(dateFields.ContractTerm, nextDay);

                setStartDate(dateFields.EnteringTerm, changed);
                setStartDate(dateFields.ContractTerm, nextDay);
                break;

            case 'EnteringTerm':
                if (!dates.DateDC || changed <= dates.DateDC) updateDatepicker(dateFields.DateDC, prevDay);
                if (!dates.DateBeginWork || changed <= dates.DateBeginWork) updateDatepicker(dateFields.DateBeginWork, prevDay);
                if (!dates.DateEndWork || changed < dates.DateEndWork) updateDatepicker(dateFields.DateEndWork, changed);
                if (!dates.ContractTerm || changed >= dates.ContractTerm) updateDatepicker(dateFields.ContractTerm, nextDay);

                setStartDate(dateFields.ContractTerm, nextDay);
                break;

            case 'ContractTerm':
                if (!dates.DateDC || changed <= dates.DateDC) updateDatepicker(dateFields.DateDC, prevTwoDay);
                if (!dates.DateBeginWork || changed <= dates.DateBeginWork) updateDatepicker(dateFields.DateBeginWork, prevTwoDay);
                if (!dates.DateEndWork || changed <= dates.DateEndWork) updateDatepicker(dateFields.DateEndWork, prevDay);
                if (!dates.EnteringTerm || changed <= dates.EnteringTerm) updateDatepicker(dateFields.EnteringTerm, prevDay);
                break;
        }

        updateMinDateLabels();
    }

    function updateMinDateLabels() {
        Object.entries(dateFields).forEach(([field, selector]) => {
            if (field === 'DateDC') return;

            const minDate = $(selector).datepicker('getStartDate');
            const value = $(selector).val();
            const label = $(`#label${field}`);

            //if ((!value || value === '') && minDate && minDate !== -Infinity) {
            //    label.html(`Минимальная дата: ${minDate.toLocaleDateString()}`).show();
            //} else {
            //    label.hide();
            //}
        });
    }

    //// Add change event listeners
    //Object.entries(dateFields).forEach(([field, selector]) => {
    //    document.querySelector(selector).addEventListener('change', () => updateDates(field));
    //});
}


///////проверка на взаимоисключающие позиции условий авансирования, если выбрано "Без авансов" - остальные замьюченные, и наоборот/////
function handleCheckboxChanges() {
    let ch1 = document.querySelector("#check0");
    let ch2 = document.querySelector("#check1");
    let ch3 = document.querySelector("#check2");

    ch1.addEventListener('change', (e) => {
        if (e.currentTarget.checked) {
            ch2.setAttribute("disabled", true);
            ch3.setAttribute("disabled", true);
        }

        if (!e.currentTarget.checked) {
            ch2.removeAttribute("disabled");
            ch3.removeAttribute("disabled");
        }
    });

    ch2.addEventListener('change', (e) => {
        if (e.currentTarget.checked) {
            ch1.setAttribute("disabled", true);
        }
        if (!e.currentTarget.checked && !ch3.checked) {
            ch1.removeAttribute("disabled");
        }
    });

    ch3.addEventListener('change', (e) => {
        if (e.currentTarget.checked) {
            ch1.setAttribute("disabled", true);
        }
        if (!e.currentTarget.checked && !ch2.checked) {
            ch1.removeAttribute("disabled");
        }
    });
}


initializeDatepickers();
handleCheckboxChanges();