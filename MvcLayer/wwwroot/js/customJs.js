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