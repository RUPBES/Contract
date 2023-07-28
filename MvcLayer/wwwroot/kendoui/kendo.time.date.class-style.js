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

//let monthpicker = function (classElem) {
//    kendo.culture("by-BY");

//    $(classElem).kendoDatePicker({

//        // defines the start view
//        start: "year",

//        // defines when the calendar should return date
//        depth: "year",

//        // display month and year in the input
//        format: "MMMM yyyy",

//        // specifies that DateInput is used for masking the input element
//        dateInput: true
//    });
//};


