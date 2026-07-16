
$(document).ready(function () {
    // Определяем разделитель по умолчанию (запятая для RU-локализации)
    let defaultSeparator = ','; 

    // Автоматически определяем, какой разделитель прислал сервер (.NET) при загрузке
    $('.cost-format').each(function () {
        let initialVal = $(this).val();
        if (initialVal.includes('.')) {
            defaultSeparator = '.';
            return false; // выходим из цикла
        } else if (initialVal.includes(',')) {
            defaultSeparator = ',';
            return false;
        }
    });

    // Основная функция форматирования и округления
    function formatCostInput(input, forceDecimals = false) {
        let val = $(input).val();
        if (!val) return;

        // Запоминаем позицию курсора (только если элемент в фокусе)
        let isFocused = (document.activeElement === input);
        let cursorStart = isFocused ? input.selectionStart : 0;
        let originalLength = val.length;

        // Очищаем строку для получения чистого JS-числа (всегда с точкой)
        let cleanVal = val.replace(/\s/g, '').replace(',', '.');
        
        let num = parseFloat(cleanVal);
        if (isNaN(num)) return;

        // Выбираем правильный разделитель для отображения
        let separator = val.includes('.') ? '.' : (val.includes(',') ? ',' : defaultSeparator);

        let integerPart = "";
        let decimalPart = "";

        if (forceDecimals) {
            // ТУТ ПРОИСХОДИТ ОКРУГЛЕНИЕ:
            // .toFixed(2) математически округляет число (например, 150.126 -> "150.13")
            let fixedVal = num.toFixed(2); 
            let parts = fixedVal.split('.');
            integerPart = parts[0];
            decimalPart = parts[1];
        } else {
            // При вводе просто разделяем целую и дробную часть, не округляя на лету
            let parts = cleanVal.split('.');
            integerPart = parts[0].replace(/[^\d-]/g, ''); // только цифры и минус
            decimalPart = parts.length > 1 ? parts[1].replace(/\D/g, '') : null; // только цифры в копейках
        }

        // Форматируем тысячи пробелами
        let integerFormatted = integerPart.replace(/\B(?=(\d{3})+(?!\d))/g, " ");

        // Собираем итоговую строку
        let newValue = integerFormatted;
        if (decimalPart !== null) {
            newValue += separator + decimalPart;
        }

        $(input).val(newValue);

        // Восстанавливаем курсор, чтобы он не прыгал в конец при вводе
        if (isFocused && !forceDecimals) {
            let newLength = newValue.length;
            let diff = newLength - originalLength;
            let newCursorPos = Math.max(0, cursorStart + diff);
            input.setSelectionRange(newCursorPos, newCursorPos);
        }
    }

    // 1. При вводе: форматируем целую часть (добавляем пробелы)
    $('.cost-format').on('input', function () {
        formatCostInput(this, false);
    });

    // 2. При потере фокуса: математически округляем до 2 знаков и дополняем нулями
    $('.cost-format').on('blur', function () {
        formatCostInput(this, true);
    });

    // 3. При загрузке страницы: округляем и красиво форматируем значения из БД
    $('.cost-format').each(function () {
        formatCostInput(this, true);
    });

    // 4. Настройка jQuery валидатора (чтобы он не ругался на пробелы и запятые в браузере)
    if ($.validator) {
        $.validator.methods.number = function (value, element) {
            let cleanValue = value.replace(/\s/g, '').replace(',', '.');
            return this.optional(element) || !isNaN(Number(cleanValue));
        };

        $.validator.methods.range = function (value, element, param) {
            let cleanValue = Number(value.replace(/\s/g, '').replace(',', '.'));
            return this.optional(element) || (cleanValue >= param[0] && cleanValue <= param[1]);
        };
    }

    // 5. Перед отправкой формы на сервер: удаляем только пробелы
    $('form').on('submit', function () {
        if ($(this).valid()) {
            $('.cost-format').each(function () {
                let cleanValue = $(this).val().replace(/\s/g, '');
                $(this).val(cleanValue);
            });
        }
    });
});