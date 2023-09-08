let datepicker = function (classElem) {
    kendo.culture("by-BY");

    $(classElem).kendoDatePicker({
        value: new Date(),
        format: "dd.MM.yyyy",
        dateInput: true
    });
};
let dateTimepicker = function (classElem) {

    kendo.culture("by-BY");

    $(classElem).kendoDateTimePicker({
        value: new Date(),
        format: "dd.MM.yyyy  H:mm",
        dateInput: true
    });
};
let datepickerNull = function (classElem) {
    kendo.culture("by-BY");

    $(classElem).kendoDatePicker({
    });
};
let datetimepickerNull = function (classElem) {
    kendo.culture("by-BY");

    $(classElem).kendoDateTimePicker({
    });
};

//let monthYearPicker = function (classElem) {
//    kendo.culture("by-BY");

//    $(classElem).kendoDatePicker({
//        value: new Date(),
//        format: "MM.yyyy",
//        depth: "year",
//        start: "year"
//    });
//};



